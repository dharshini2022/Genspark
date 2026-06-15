                    +---------------------+
                    |      .env File       |
                    |---------------------|
                    | Email Credentials   |
                    | Subject             |
                    | Gemini API Key      |
                    +----------+----------+
                               |
                               v
                    +---------------------+
                    |  Configuration Layer |
                    | (.env Loader)        |
                    +----------+----------+
                               |
                               v
                 +---------------------------+
                 | Email Service (IMAP)      |
                 |---------------------------|
                 | Search Subject:           |
                 | "Client Project Requirement" |
                 +------------+--------------+
                              |
                              v
                +-----------------------------+
                | Download Email Content       |
                | Save as requirements.txt     |
                +-------------+---------------+
                              |
                              v
                 +----------------------------+
                 | Requirement Processing     |
                 |----------------------------|
                 | Parse Raw Client Text       |
                 | Extract Requirements        |
                 +-------------+--------------+
                               |
                               v
               +---------------------------------+
               | Gemini API / BRD Generation     |
               |---------------------------------|
               | Functional Requirements         |
               | Non-functional Requirements     |
               | Risks                           |
               | Assumptions                     |
               | Questions to Client             |
               +-------------+-------------------+
                             |
                             v
                +-----------------------------+
                | Generate Structured BRD     |
                +-------------+---------------+
                              |
                              v
                  +--------------------------+
                  | Email Service (SMTP)     |
                  |--------------------------|
                  | Send Original Content    |
                  | Back to Sender           |
                  +-------------+------------+
                                |
                                v
                      +------------------+
                      | Final Output BRD |
                      +------------------+


This architecture follows a layered design with clear separation between configuration, email handling, requirement analysis, AI processing, and notification components, making it suitable for enterprise-grade implementations.