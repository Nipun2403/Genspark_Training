import google.generativeai as genai
from config import API_KEY_GEMINI

class LLMClient:
    def __init__(self, model_name: str = "gemini-2.5-flash"):
        if not API_KEY_GEMINI:
            raise ValueError("Environment variable GEMINI_API_KEY is not set.")
            
        genai.configure(api_key=API_KEY_GEMINI)
        self.llm = genai.GenerativeModel(model_name)

    def process_prompt(self, input_text: str) -> str:
        try:
            res = self.llm.generate_content(input_text)
            if not res.text:
                raise ValueError("Received an empty string from the model.")
            return res.text
        except Exception as e:
            raise RuntimeError(f"Error calling Generative AI: {e}")