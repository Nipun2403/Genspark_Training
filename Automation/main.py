import json
import sys
from pathlib import Path

from prompt_builder import PromptGenerator
from gemini_service import LLMClient
from email_service import NotificationService

def parse_llm_json(raw_text: str) -> dict:
    """Helper function to extract and parse JSON from the LLM output."""
    try:
        text = raw_text.strip()
        if text.startswith("```json"):
            text = text.removeprefix("```json").removesuffix("```").strip()
        elif text.startswith("```"):
            text = text.removeprefix("```").removesuffix("```").strip()
            
        return json.loads(text)
    except json.JSONDecodeError as err:
        raise ValueError(f"Failed to parse JSON string: {err}")

def main():
    try:
        print("Starting requirement analysis process...")
        
        # 1. Generate Prompt
        generator = PromptGenerator(
            template_path="prompts/requirement_analysis.prompt",
            doc_path="requirements/client_requirement.docx"
        )
        prompt_text = generator.create_final_prompt()
        print("[OK] Prompt text constructed.")

        # 2. Call LLM
        client = LLMClient()
        raw_output = client.process_prompt(prompt_text)
        print("[OK] LLM response received.")

        # 3. Parse output using the injected parser function
        parsed_data = parse_llm_json(raw_output)
        
        # Ensure output directory exists
        out_dir = Path("outputs")
        out_dir.mkdir(exist_ok=True)

        # 4. Save results
        json_path = out_dir / "analysis.json"
        json_path.write_text(json.dumps(parsed_data, indent=2), encoding="utf-8")
        print(f"[OK] JSON output saved to {json_path}")

        log_path = out_dir / "gemini_conversation.txt"
        log_content = (
            f"--- PROMPT ---\n{prompt_text}\n\n"
            f"--- RESPONSE ---\n{raw_output}\n"
        )
        log_path.write_text(log_content, encoding="utf-8")
        print(f"[OK] Conversation log saved to {log_path}")

        # 5. Send Email
        mailer = NotificationService()
        project_title = parsed_data.get('project_name', 'Project')
        html_body = mailer.construct_html_body(parsed_data)
        
        mailer.dispatch_email(
            mail_subject=f"Analysis Results: {project_title}",
            html_content=html_body
        )
        print("[OK] Workflow completed successfully.")

    except Exception as e:
        print(f"[ERROR] Process failed: {e}", file=sys.stderr)
        sys.exit(1)

if __name__ == "__main__":
    main()