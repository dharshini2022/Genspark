# ERROR HANDLING
Errors are handled through out the python script using try catch block.
---
# Edge Cases Checked

1. Configuration & Initialization Edge Cases
    * Missing .env Variables: If an environment variable (like GEMINI_API_KEY or SENDER_EMAIL) is completely missing or left blank, validate_env_variables() catches it immediately before executing any network logic, preventing vague downstream failures.
2. Email Fetching (imaplib) Edge Cases
    * No Matching Emails Found: If no email matches your search criteria, the script returns False gracefully and halts execution instead of crashing when attempting to access an empty list (mail_ids[-1]).
    * Search or Fetch Failures: If Gmail rejects the search query or fails to fetch the specific message body due to an intermittent server glitch, the script checks the server status flags (status != 'OK') and aborts cleanly.
    * Multipart vs. Singlepart Emails: Email clients structure data differently depending on formatting. The script explicitly handles both msg.is_multipart() structures (looping to extract only text/plain) and single-part plain-text payloads.
    * Empty Email Bodies: If an email is found with the correct subject line but contains absolutely no text, the script detects the empty string and stops the pipeline before passing it to Gemini.
    * Dangling Network Connections: If the script encounters an error mid-stream while processing email data, the finally: block ensures that the IMAP session is closed and logged out cleanly, preventing unclosed socket leaks.
3. File System (IOError) Edge Cases
    * Missing Requirements File: In read_requirement_file(), if the file was deleted or cannot be found due to OS permissions, a specific FileNotFoundError is caught and logged, preventing the pipeline from sending non-existent data to Gemini.
    * Write/Read Failures: Disk-full errors or locked files during the writing of requirements.txt are caught via IOError.
4. AI Generation (google-genai) Edge Cases
    * Gemini API Failures: Captures APIError explicitly. This handles issues like invalid API keys, hitting rate limits (TPM/RPM), quota exhaustion, or temporary Google server outages.
    * Empty AI Responses: If the API successfully communicates but returns an empty response body (e.g., due to a safety filter block or a generation glitch), the script explicitly validates response.text and raises a ValueError.
5. Email Transmission (smtplib) Edge Cases
    * Gmail App Password Issues: Captures SMTPAuthenticationError. This specifically flags when your credentials are rejected—a common issue if two-factor authentication (2FA) is enabled on Gmail but a standard password was used instead of an App Password.
    * Network/Mailing Failures: Handles general SMTPException issues, such as a sudden loss of internet connection, dropped server connections mid-send, or if the RECEIVER_EMAIL is rejected by the mail server.