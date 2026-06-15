import os
from pathlib import Path
from docx import Document

class PromptGenerator:
    def __init__(self, template_path: str, doc_path: str):
        self.template_path = template_path
        self.doc_path = doc_path

    def fetch_template(self) -> str:
        try:
            return Path(self.template_path).read_text(encoding="utf-8")
        except FileNotFoundError as err:
            raise FileNotFoundError(f"Could not locate prompt file: {self.template_path}") from err
        except Exception as err:
            raise RuntimeError(f"Error loading prompt file: {err}") from err

    def fetch_document(self) -> str:
        try:
            p = Path(self.doc_path)
            if p.suffix == ".docx":
                doc = Document(self.doc_path)
                lines = [paragraph.text for paragraph in doc.paragraphs]
                return "\n".join(lines)
            return p.read_text(encoding="utf-8")
        except FileNotFoundError as err:
            raise FileNotFoundError(f"Could not locate requirement document: {self.doc_path}") from err
        except Exception as err:
            raise RuntimeError(f"Error reading document: {err}") from err

    def combine(self, template_str: str, req_str: str) -> str:
        if not template_str.strip():
            raise ValueError("The prompt template provided is empty.")
        if not req_str.strip():
            raise ValueError("The requirement text is empty.")
        
        target_placeholder = "{{requirement}}"
        if target_placeholder not in template_str:
            raise ValueError(f"Missing placeholder {target_placeholder} in the template.")
            
        return template_str.replace(target_placeholder, req_str)

    def create_final_prompt(self) -> str:
        try:
            base_template = self.fetch_template()
            requirements = self.fetch_document()
            return self.combine(base_template, requirements)
        except Exception as err:
            raise RuntimeError(f"Failed to generate prompt: {err}") from err