### SYSTEM SPECIFICATION ###
Engine: ResumeGeneratorEngine v2.0
Role: TechnicalRecruiter | StructuralWriter
Target: ATS-Optimized Single Page Resume (Software Engineer)

### CONFIGURATION SETTINGS ###
Layout: Single-column, zero text-boxes, zero graphical bars
Typography: font-family: "Calibri", body-size: 11pt, title-size: 12pt (Bold)
ColorPalette: Primary="#1F4E78" (Deep Navy), Body="#333333" (Charcoal), Meta="#595959" (Slate)
Spacing: line-spacing: 1.15, space-after-bullet: 3pt
OutputFormat: Docs

### DATA SCHEMA INPUT ###
const Profile = {
  Candidate: "Dharshini Karthik",
  Meta: "Coimbatore, India | linkedin.com/in/dharshini-karthik-414278248",
  
  Skills: {
    Backend: [".NET Core (WebAPI, MVC)", "Spring Boot", "Node.js", "Express.js"],
    Frontend: ["Angular", "React", "HTML5", "CSS3", "JavaScript"],
    Databases: ["PostgreSQL", "SQL", "MongoDB"],
    Languages: ["C#", "Java", "JavaScript"]
  },
  
  Experience: [
    { role: "Associate Engineer Intern", company: "Presidio", date: "March 2026 – Present", description: "Building .NET Full Stack applications utilizing WebAPI, PostgreSQL, and Angular." },
    { role: "Software Project Intern", company: "Roots", date: "November 2025 – February 2026", description: "Managed hardware operations integrated software maintenance using the .NET MVC tech stack." },
    { role: "Open Source Contributor", company: "GSSOC '24", date: "December 2024", description: "Contributed to public codebases." }
  ],
  
  Projects: [
    { name: "ADHD Detection & Explainable AI (XAI)", tech: "CNN, Saliency Maps", summary: "Classification model with transparent, interpretable visual explanations." },
    { name: "End-to-End Pet Adoption System", tech: "Spring Boot, React, SQL", summary: "Full functionality system featuring integrated workflows." },
    { name: "Emergency Response Medical System", tech: "MERN Stack, C#, APIs", summary: "Platform with native fingerprint integration, Geolocation tracking, Fast2SMS, and Google Translation API." }
  ],
  
  PublicationsAndAchievements: [
    "Research Paper (IEEE): Published an IoT-based Aquarium Maintenance application.",
    "Research Paper (SCITEPRESS): Published on ADHD Classification using deep learning.",
    "Hackathons & Expos: Winner at Project Expo (2023, 2024) and InsightX Hackathon (2023). Finalist at Reva SheCodes Hackathon (2024)."
  ]
};

### EXECUTION PIPELINE ###
1. Map `Profile.Candidate` and `Profile.Meta` to centered document header.
2. Compile a 3-4 sentence "Professional Summary" maximizing keyword overlaps with Full Stack and Research domains.
3. Map `Profile.Skills` into inline categorized list structures.
4. Format `Profile.Experience` and `Profile.Projects` into modern Left-Title / Right-Date tables or tab-stopped layouts. 
5. Translate raw descriptions using the Strong-Action-Verb + Impact format. Max 2 bullets per node.
6. Assemble into runnable script compiling directly to "Dharshini_Karthik_Resume.docx".