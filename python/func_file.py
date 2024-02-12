import asyncio
import logging
import random

import requests
import json
from io import BytesIO

from typing import Optional

from aiogram import Bot, Dispatcher, types, executor 
from aiogram.contrib.middlewares.logging import LoggingMiddleware
from aiogram.dispatcher import FSMContext
from aiogram.dispatcher.filters.state import State, StatesGroup
from aiogram.contrib.fsm_storage.memory import MemoryStorage

import main
from bot_tokens import BOT_TOKEN, hi_stickers, server_url

bot = Bot(token = BOT_TOKEN)
storage = MemoryStorage()
dp = Dispatcher(bot, storage=storage)
dp.middleware.setup(LoggingMiddleware())


# Handling the /help command
@dp.message_handler(commands=["help"])
async def help(message: types.Message):
    await bot.send_message(chat_id=message.from_user.id, text="File-creating Bot\n\nAvailable commands:\n/start - get started with the bot\n/help - bot help\n/reset - finish processing or reset parameters")

# Handling the /reset command
@dp.message_handler(state='*', commands='reset')
async def process_reset(message: types.Message, state: FSMContext):
    
    # Allow user to cancel any action
    current_state = await state.get_state()
    if current_state is None:
        return
    logging.info('Reset state %r', current_state)

    # Cancel state and inform user about it
    await state.finish()

    # Remove keyboard
    await message.reply('Reset.', reply_markup=types.ReplyKeyboardRemove())


# Declaration of states
class FileForm(StatesGroup):
    text = State()
    language = State()
    another_language = State()
    font = State()
    contents = State()
    file_name = State()

# Handling the /start command
@dp.message_handler(commands=['start'])
async def get_text(message: types.Message, state: FSMContext):

    # Setting the initial state - note to be processed
    await FileForm.text.set()
    await message.answer("Send text for processing: ")

# Handling a message with the note input
@dp.message_handler(state=FileForm.text)
async def process_text(message: types.Message, state: FSMContext):

    # Getting the typed text and saving it
    user_text = message.text

    # Saving the typed note into the state
    await state.update_data(text=user_text)

    # Transitioning to the next state - choosing a font
    await FileForm.next()
    await process_language(message)

async def process_language(message: types.Message):

    # Sending a message with buttons for selecting a font
    keyboard = types.InlineKeyboardMarkup(row_width=1)
    keyboard.add(types.InlineKeyboardButton(text="English", callback_data="language_English"),
                 types.InlineKeyboardButton(text="Russian", callback_data="language_Russian"),
                 types.InlineKeyboardButton(text="Another", callback_data="language_Another"))
    await message.answer("Choose language:", reply_markup=keyboard)

# Handling the callback query for choosing language
@dp.callback_query_handler(lambda c: c.data.startswith('language_'), state=FileForm.language)
async def process_choosen_language(callback_query: types.CallbackQuery, state: FSMContext):
    # Getting the chosen language from the callback query
    chosen_language = callback_query.data.split('_')[1]

    if(chosen_language == "Another"):
        await callback_query.message.answer("Write the language:")

        await state.set_state(FileForm.another_language)
        await process_another_language(callback_query.message)

    # Saving the chosen font into the state
    else: 
        await state.update_data(language=chosen_language)

        await state.set_state(FileForm.font)
        await process_font(callback_query.message)



# Handling a message with the another language input
@dp.message_handler(state=FileForm.another_language)
async def process_another_language(message: types.Message, state: FSMContext):

    # Getting the entered file name and saving it
    user_language = message.text
    await state.update_data(language=user_language)

    await state.set_state(FileForm.font)
    await process_font(message)

# Handling a message with buttons for selecting a font
async def process_font(message: types.Message):
        
    # Sending a message with buttons for selecting a font
    keyboard = types.InlineKeyboardMarkup(row_width=1)
    keyboard.add(types.InlineKeyboardButton(text="Arial", callback_data="font_Arial"),
                 types.InlineKeyboardButton(text="Calibri", callback_data="font_Calibri"),
                 types.InlineKeyboardButton(text="Times New Roman", callback_data="font_TimesNewRoman"))
    await message.answer("Choose a font:", reply_markup=keyboard)

# Handling the callback query for choosing a font
@dp.callback_query_handler(lambda c: c.data.startswith('font_'), state=FileForm.font)
async def process_choosen_font(callback_query: types.CallbackQuery, state: FSMContext):

    # Getting the chosen font from the callback query
    chosen_font = callback_query.data.split('_')[1]

    # Saving the chosen font into the state
    await state.update_data(font=chosen_font)

    # Transitioning to the next state - choosing content availability
    await state.set_state(FileForm.contents)   

    # Sending a message with buttons for choosing content availability
    await process_contents(callback_query.message)
    
# Handling a message with buttons for choosing content availability
async def process_contents(message: types.Message):
    keyboard = types.InlineKeyboardMarkup(row_width=1)
    keyboard.add(types.InlineKeyboardButton(text="Yes", callback_data="contents_yes"),
                 types.InlineKeyboardButton(text="No", callback_data="contents_no"))
    await message.answer(text="Do you need contents?", reply_markup=keyboard)

# Handling the callback query for choosing content availability
@dp.callback_query_handler(lambda c: c.data.startswith('contents_'), state=FileForm.contents)
async def process_choosen_contents(callback_query: types.CallbackQuery, state: FSMContext):

    # Getting the chosen content availability from the callback query
    chosen_contents = callback_query.data.split('_')[1]

    # Saving the chosen option into the state
    await state.update_data(contents=chosen_contents)
    # Transitioning to the next state - entering the file name
    await FileForm.next()

    # Sending a message requesting the file name input
    await callback_query.message.answer("Enter file name:")

# Handling a message with the file name input
@dp.message_handler(state=FileForm.file_name)
async def process_file_name(message: types.Message, state: FSMContext):

    # Getting the entered file name and saving it
    origin_file_name = message.text
    file_name = origin_file_name.split('.')[0]

    await state.update_data(file_name=file_name)

    user_state_data = await state.get_data()

    # Sending a message with the collected data
    await message.answer(text = "Your file is being generated.\nPlease wait")

    #Serialization of data into a JSON string with specifying the utf-8 encoding
    json_string = json.dumps(user_state_data, ensure_ascii=False).encode('utf-8')

    #Specifying the Content-Type header and utf-8 encoding  
    headers = {"Content-Type": "application/json; charset=utf-8"}

    #Sending data via API
    try:
        response = requests.post(server_url, data = json_string, headers = headers)

        if response.status_code == 200:

            print("Data sent successfully")
            stream = BytesIO(response.content)

            await bot.send_document(message.from_user.id, document = (file_name + ".docx", stream))

    except Exception as e:
        print(f"Failed to send data.{e}")
        await bot.send_message(message.from_user.id, "We are having problems.\nPlease try later")


    # Finishing the FSM (clearing states)
    await state.finish()

 
# Handling a message with the content_types="text" 
# hi, hello, undefined
@dp.message_handler(content_types="text")
async def get_default_message(message: types.Message):

    if message.text.lower() == "hi" or message.text.lower() == "hello":
        await bot.send_sticker(chat_id= message.from_user.id,sticker= hi_stickers[(random.randint(1, 1000000)) % len(hi_stickers)])
        await bot.send_message(message.from_user.id, "Hello!\nI will help you take notes and put them into a Word document\nWrite /start to get started")
    else:
        await bot.send_message(message.from_user.id, "I don't understand you, write /help.")

