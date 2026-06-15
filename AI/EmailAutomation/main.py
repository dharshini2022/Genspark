import os
import smtplib
import imaplib
import email
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from email.header import decode_header
from dotenv import load_dotenv
from google import genai
from google.genai.errors import APIError  # Import specific Gemini API errors

load_dotenv()

GEMINI_API_KEY = os.getenv("GEMINI_API_KEY")
SENDER_EMAIL = os.getenv("SENDER_EMAIL")
SENDER_PASSWORD = os.getenv("SENDER_PASSWORD")
RECEIVER_EMAIL = os.getenv("RECEIVER_EMAIL")
EMAIL_SUBJECT_LOOKUP = os.getenv("EMAIL_SUBJECT_LOOKUP")

def validate_env_variables():
    """Validates that all required environment variables are set."""
    required_vars = {
        "GEMINI_API_KEY": GEMINI_API_KEY,
        "SENDER_EMAIL": SENDER_EMAIL,
        "SENDER_PASSWORD": SENDER_PASSWORD,
        "RECEIVER_EMAIL": RECEIVER_EMAIL,
        "EMAIL_SUBJECT_LOOKUP": EMAIL_SUBJECT_LOOKUP
    }
    missing = [key for key, val in required_vars.items() if not val]
    if missing:
        raise ValueError(f"Missing required environment variables: {', '.join(missing)}")

def fetch_client_email_to_file():
    print(" Checking Gmail inbox for new client requirements...")
    mail = None
    try:
        mail = imaplib.IMAP4_SSL("imap.gmail.com")
        mail.login(SENDER_EMAIL, SENDER_PASSWORD)
        mail.select("inbox")
        
        status, messages = mail.search(None, f'SUBJECT "{EMAIL_SUBJECT_LOOKUP}"')
        if status != 'OK':
            print(" Failed to search emails.")
            return False
            
        mail_ids = messages[0].split()
        if not mail_ids:
            print(" No matching requirement emails found in the inbox.")
            return False

        latest_mail_id = mail_ids[-1]
        status, data = mail.fetch(latest_mail_id, "(RFC822)")
        if status != 'OK':
            print("Failed to fetch the target email.")
            return False
            
        raw_email = data[0][1]
        msg = email.message_from_bytes(raw_email)
        
        email_body = ""
        if msg.is_multipart():
            for part in msg.walk():
                content_type = part.get_content_type()
                if content_type == "text/plain":
                    payload = part.get_payload(decode=True)
                    if payload:
                        email_body = payload.decode('utf-8', errors='ignore')
                        break
        else:
            payload = msg.get_payload(decode=True)
            if payload:
                email_body = payload.decode('utf-8', errors='ignore')
            
        if email_body.strip():
            with open("requirements.txt", "w", encoding="utf-8") as file:
                file.write(email_body.strip())
            print(" Successfully downloaded email content into requirements.txt!")
            return True
        else:
            print(" email was found, but the body content appears to be empty.")
            return False
            
    except imaplib.IMAP4.error as e:
        print(f" IMAP Mail Error: Connection or authentication failed. Details: {e}")
        return False
    except IOError as e:
        print(f" File Write Error: Could not save requirements.txt. Details: {e}")
        return False
    except Exception as e:
        print(f" Unexpected Error during email fetching: {e}")
        return False
    finally:
        if mail:
            try:
                mail.close()
                mail.logout()
            except Exception:
                pass  


def read_requirement_file(file_path):
    print(f"Reading {file_path}...")
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            return file.read()
    except FileNotFoundError:
        print(f" Error: The file '{file_path}' was not found.")
        raise
    except IOError as e:
        print(f" Error: Could not read file '{file_path}'. Details: {e}")
        raise


def generate_requirements_with_gemini(client_text):
    print("Sending text to Gemini for analysis...")
    if not client_text.strip():
        raise ValueError("Input client text is empty. Cannot analyze with Gemini.")
        
    try:
        client = genai.Client(api_key=GEMINI_API_KEY)
        
        prompt = (
            "You are an expert Business Analyst. Analyze the provided client requirement text "
            "and strictly generate a standardized report with these 5 sections:\n"
            "1. Functional Requirements\n"
            "2. Non-functional Requirements\n"
            "3. Risks\n"
            "4. Assumptions\n"
            "5. Questions to Client\n\n"
            "Format the output using clean HTML (use <h2> for headers and <ul>/<li> for lists) "
            "so it looks beautiful and professional in an email. Do not include introductory text like 'Here is your report'."
        )
        
        response = client.models.generate_content(
            model='gemini-2.5-flash',
            contents=[prompt, client_text]
        )
        
        if not response.text:
            raise ValueError("Gemini returned an empty response.")
            
        return response.text

    except APIError as e:
        print(f" Gemini API Error: The model request failed. Details: {e}")
        raise
    except Exception as e:
        print(f" Unexpected Error during Gemini generation: {e}")
        raise


def send_email(html_content):
    print(f"Sending formatted email to {RECEIVER_EMAIL}...")
    try:
        msg = MIMEMultipart('alternative')
        msg['Subject'] = 'Standardized Project Requirements Analysis'
        msg['From'] = SENDER_EMAIL
        msg['To'] = RECEIVER_EMAIL
        
        part = MIMEText(html_content, 'html')
        msg.attach(part)
        
        with smtplib.SMTP_SSL('smtp.gmail.com', 465) as server:
            server.login(SENDER_EMAIL, SENDER_PASSWORD)
            server.sendmail(SENDER_EMAIL, RECEIVER_EMAIL, msg.as_string())
        
        print(" Email sent successfully!")
        
    except smtplib.SMTPAuthenticationError:
        print(" SMTP Authentication Error: Check your SENDER_EMAIL and SENDER_PASSWORD (App Password).")
        raise
    except smtplib.SMTPException as e:
        print(f" SMTP Error: Failed to send email via network. Details: {e}")
        raise
    except Exception as e:
        print(f" Unexpected Error while sending email: {e}")
        raise


if __name__ == "__main__":
    try:
        validate_env_variables()
        
        email_found = fetch_client_email_to_file()
        
        if email_found:
            raw_text = read_requirement_file("requirements.txt")
            
            structured_report = generate_requirements_with_gemini(raw_text)
            
            send_email(structured_report)
            
            print("\n Full Loop Automation Complete! Check your inbox.")
        else:
            print("\n Automation halted: No valid matching email was found or processed.")
            
    except ValueError as ve:
        print(f"\n Configuration/Validation Error: {ve}")
    except Exception as e:
        print(f"\n Critical Failure: The pipeline could not finish. Error: {e}")