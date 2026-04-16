import os
from dotenv import load_dotenv

# Загружаем переменные из файла .env
load_dotenv()

# API ключ OpenRouter
OPENROUTER_API_KEY = os.getenv("OPENROUTER_API_KEY")
SELECTED_MODEL = "openrouter/elephant-alpha"

# Адрес OpenRouter
OPENROUTER_URL = "https://openrouter.ai/api/v1/chat/completions"