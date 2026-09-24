-- Silverland CRM Database Initialization Schema Script
CREATE TABLE IF NOT EXISTS "Users" (
    "Id" UUID PRIMARY KEY,
    "Name" VARCHAR(150) NOT NULL,
    "Email" VARCHAR(150) UNIQUE NOT NULL,
    "Mobile" VARCHAR(20) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" INTEGER NOT NULL,
    "IsLoginEnabled" BOOLEAN DEFAULT TRUE,
    "IsDeleted" BOOLEAN DEFAULT FALSE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS "Leads" (
    "Id" UUID PRIMARY KEY,
    "LeadDate" VARCHAR(20) NOT NULL,
    "LeadTime" VARCHAR(20) NOT NULL,
    "LeadName" VARCHAR(150) NOT NULL,
    "MobileNumber" VARCHAR(20) NOT NULL,
    "EnquiredOn" VARCHAR(150) NOT NULL,
    "Response" VARCHAR(100) NOT NULL,
    "ResponseDetails" TEXT,
    "LeadSource" VARCHAR(100) DEFAULT 'Manual',
    "AssignedEmployeeId" UUID REFERENCES "Users"("Id"),
    "CreatedAt" TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_Leads_MobileNumber" ON "Leads"("MobileNumber");
