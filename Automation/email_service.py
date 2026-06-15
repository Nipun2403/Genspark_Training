import smtplib
from email.message import EmailMessage

from config import SENDER_EMAIL, SENDER_PWD, DESTINATION_EMAIL

class NotificationService:
    def dispatch_email(self, mail_subject: str, html_content: str):
        try:
            msg = EmailMessage()
            msg["Subject"] = mail_subject
            msg["From"] = SENDER_EMAIL
            msg["To"] = DESTINATION_EMAIL

            msg.add_alternative(html_content, subtype="html")
            
            with smtplib.SMTP_SSL("smtp.gmail.com", 465) as server:
                server.login(SENDER_EMAIL, SENDER_PWD)
                server.send_message(msg)

            print("Notification dispatched successfully.")
        except Exception as err:
            raise RuntimeError(f"Could not send email: {err}")

    @staticmethod
    def construct_html_body(data: dict) -> str:
        def make_ul(items):
            list_items = "".join([f"<li>{i}</li>" for i in items])
            return f"<ul>{list_items}</ul>"
        
        html = f"""
        <html>
        <head>
            <style>
                body {{ font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; background-color: #e9ecef; padding: 20px; }}
                .container {{ max-width: 800px; margin: 0 auto; background: #ffffff; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                .header {{ background-color: #2c3e50; color: #ffffff; padding: 25px; border-radius: 8px 8px 0 0; text-align: center; }}
                .content {{ padding: 30px; color: #333333; line-height: 1.6; }}
                h2 {{ border-bottom: 2px solid #ecf0f1; padding-bottom: 5px; margin-top: 25px; }}
                .func-req {{ color: #2980b9; }}
                .non-func {{ color: #8e44ad; }}
                .risks {{ color: #e74c3c; }}
                .assumptions {{ color: #27ae60; }}
                .questions {{ color: #f39c12; }}
                .footer {{ background: #f8f9fa; padding: 15px; text-align: center; font-size: 12px; color: #7f8c8d; border-radius: 0 0 8px 8px; margin-top: 20px; }}
            </style>
        </head>
        <body>
            <div class="container">
                <div class="header">
                    <h2>System Analysis Results</h2>
                    <p>Project: {data.get('project_name', 'Insurance Claim System')}</p>
                </div>
                <div class="content">
                    <p>Hello,</p>
                    <p>The automated analysis of the requirements document is complete. Below are the extracted details:</p>

                    <h2 class="func-req">1. Functional Requirements</h2>
                    {make_ul(data.get("functional_requirements", []))}

                    <h2 class="non-func">2. Non-Functional Requirements</h2>
                    {make_ul(data.get("non_functional_requirements", []))}

                    <h2 class="risks">3. Potential Risks</h2>
                    {make_ul(data.get("risks", []))}

                    <h2 class="assumptions">4. Assumptions Made</h2>
                    {make_ul(data.get("assumptions", []))}

                    <h2 class="questions">5. Open Clarifications</h2>
                    {make_ul(data.get("questions", []))}

                    <p>Please review these points so we can proceed with the next phase.</p>
                    <p>Best,<br><strong>Automated Analysis Bot</strong></p>
                </div>
                <div class="footer">
                    &copy; 2024 Presidio Automations
                </div>
            </div>
        </body>
        </html>
        """
        return html
