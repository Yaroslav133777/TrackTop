from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import requests
import json
import re
from config import OPENROUTER_API_KEY, SELECTED_MODEL, OPENROUTER_URL

# =============================================
# ИНИЦИАЛИЗАЦИЯ ПРИЛОЖЕНИЯ
# =============================================

app = FastAPI(
    title="TrackTop AI Service",
    description="ИИ-микросервис для парсинга задач из текста",
    version="0.1.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


# =============================================
# МОДЕЛИ ДАННЫХ (Контракт с бэкендом)
# =============================================

# Что Бэкенд присылает
class ParseRequest(BaseModel):
    text: str
    current_datetime: str  # Текущее время с устройства пользователя

    # Бэк пишет: DateTime.Now.ToString("o")
    # Пример: "2026-03-05T14:30:00.0000000+03:00"

    class Config:
        json_schema_extra = {
            "example": {
                "text": "Напомни завтра в 15:00 купить хлеб и молоко",
                "current_datetime": "2026-03-05T14:30:00+03:00"
            }
        }


# Что возвращаем Бэкенду
class ParseResponse(BaseModel):
    label: str  # Название задачи
    description: str | None = None  # Детали (если есть)
    notification: bool  # true = задача с уведомлением, false = просто заметка
    timeToNotify: str | None = None  # ISO 8601 или null
    notificationType: str  # "once", "daily", "weekly"

    class Config:
        json_schema_extra = {
            "example": {
                "label": "Купить хлеб и молоко",
                "description": None,
                "notification": True,
                "timeToNotify": "2026-03-06T15:00:00",
                "notificationType": "once"
            }
        }


# =============================================
# ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
# =============================================

def build_prompt(text: str, current_datetime: str) -> str:
    """Формирует промпт для ИИ"""
    return f"""
You are a smart task planning assistant. Your job is to extract task information from user input.
Current date and time: {current_datetime}
Use this to resolve relative dates like "tomorrow", "next week", "in 3 days".

Extract these fields:
1. `label`: Short task title. Clean, no filler words.
2. `description`: Extra details if present. null if none.
3. `notification`: true if user wants a reminder. false if just a note (no date/time mentioned).
4. `timeToNotify`: Date and time in ISO 8601 format. null if notification is false or no time given.
5. `notificationType`: "once" for one-time, "daily" for every day, "weekly" for every week.

IMPORTANT: Return ONLY valid JSON. No explanations. No markdown. No code blocks.

Examples:
Input: "Remind me to call mom tomorrow at 3pm"
Output: {{"label": "Call mom", "description": null, "notification": true, "timeToNotify": "2026-03-06T15:00:00", "notificationType": "once"}}

Input: "Every morning at 8 remind me to drink water"
Output: {{"label": "Drink water", "description": null, "notification": true, "timeToNotify": "2026-03-06T08:00:00", "notificationType": "daily"}}

Input: "Just note that I need to buy a gift for John"
Output: {{"label": "Buy a gift for John", "description": null, "notification": false, "timeToNotify": null, "notificationType": "once"}}

Input: "Buy groceries: milk, bread, eggs. Remind me today at 6pm"
Output: {{"label": "Buy groceries", "description": "Milk, bread, eggs", "notification": true, "timeToNotify": "2026-03-05T18:00:00", "notificationType": "once"}}

Now process this input:
Input: "{text}"
Output:
"""


def call_openrouter(prompt: str) -> str:
    """Отправляет запрос в OpenRouter и возвращает сырой ответ модели"""

    if not OPENROUTER_API_KEY:
        raise HTTPException(
            status_code=500,
            detail="API ключ OpenRouter не найден. Проверь файл .env"
        )

    headers = {
        "Authorization": f"Bearer {OPENROUTER_API_KEY}",
        "HTTP-Referer": "http://localhost:8000",
        "X-Title": "TrackTop AI Service",
        "Content-Type": "application/json"
    }

    payload = {
        "model": SELECTED_MODEL,
        "messages": [
            {"role": "user", "content": prompt}
        ],
        "temperature": 0.1  # Низкая температура = более предсказуемый JSON
    }

    response = requests.post(
        OPENROUTER_URL,
        headers=headers,
        data=json.dumps(payload),
        timeout=30  # Ждём ответа не дольше 30 секунд
    )

    response.raise_for_status()
    return response.json()["choices"][0]["message"]["content"]


def extract_json(raw_text: str) -> dict:
    """Извлекает JSON из ответа модели (на случай если модель добавит лишний текст)"""

    # Убираем markdown-блоки если модель их добавила (```json ... ```)
    raw_text = re.sub(r'```json\s*', '', raw_text)
    raw_text = re.sub(r'```\s*', '', raw_text)

    # Ищем JSON-объект в тексте
    json_match = re.search(r'\{.*\}', raw_text, re.DOTALL)

    if not json_match:
        raise ValueError(f"ИИ не вернул JSON. Ответ модели: {raw_text}")

    return json.loads(json_match.group())


def validate_and_fix(data: dict) -> dict:
    """Проверяет и исправляет данные от ИИ"""

    # Если нет уведомления — убираем время
    if not data.get("notification", True):
        data["timeToNotify"] = None
        data["notificationType"] = "once"

    # Если нет типа — ставим по умолчанию
    if "notificationType" not in data:
        data["notificationType"] = "once"

    # Проверяем допустимые значения notificationType
    allowed_types = ["once", "daily", "weekly"]
    if data.get("notificationType") not in allowed_types:
        data["notificationType"] = "once"

    return data


# =============================================
# ЭНДПОИНТЫ
# =============================================

@app.get("/")
def root():
    """Проверка работоспособности сервера"""
    return {
        "status": "ok",
        "service": "TrackTop AI",
        "version": "0.1.0",
        "model": SELECTED_MODEL
    }


@app.post("/api/parse-task", response_model=ParseResponse)
def parse_task(request: ParseRequest):
    """
    Основной эндпоинт.
    Принимает текст задачи, возвращает структурированный JSON.
    Бэк стучится сюда из MAUI.
    """

    try:
        # 1. Строим промпт
        prompt = build_prompt(request.text, request.current_datetime)

        # 2. Отправляем в OpenRouter
        raw_response = call_openrouter(prompt)

        # 3. Извлекаем JSON
        parsed_data = extract_json(raw_response)

        # 4. Проверяем и исправляем
        validated_data = validate_and_fix(parsed_data)

        # 5. Возвращаем результат
        return ParseResponse(**validated_data)

    except requests.exceptions.Timeout:
        raise HTTPException(
            status_code=504,
            detail="OpenRouter не ответил за 30 секунд. Попробуй ещё раз."
        )
    except requests.exceptions.RequestException as e:
        raise HTTPException(
            status_code=502,
            detail=f"Ошибка соединения с OpenRouter: {str(e)}"
        )
    except (json.JSONDecodeError, ValueError) as e:
        raise HTTPException(
            status_code=422,
            detail=f"ИИ вернул неверный формат: {str(e)}"
        )


@app.get("/health")
def health_check():
    return {"status": "healthy", "model": SELECTED_MODEL}