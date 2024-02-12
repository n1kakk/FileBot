import asyncio
import logging

import func_file
from bot_tokens import *

async def main():
    logging.basicConfig(level=logging.INFO)
    await func_file.dp.start_polling()

# Running the bot
if __name__ == "__main__":
    asyncio.run(main())
