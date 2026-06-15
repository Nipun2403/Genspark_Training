import os
from dotenv import load_dotenv

# Initialize dotenv to load environment variables
load_dotenv()

API_KEY_GEMINI = os.getenv("GEMINI_API_KEY")
SENDER_EMAIL = os.getenv("EMAIL_ADDRESS")
SENDER_PWD = os.getenv("EMAIL_PASSWORD")
DESTINATION_EMAIL = os.getenv("RECEIVER_EMAIL")