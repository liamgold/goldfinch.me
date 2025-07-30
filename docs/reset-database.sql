BEGIN TRANSACTION;

-- Update the domain for the specified website channel
UPDATE [CMS_WebsiteChannel] 
SET WebsiteChannelDomain = N'localhost:52623' 
WHERE WebsiteChannelChannelID = 1;

-- Clear the CMS license key value
UPDATE [CMS_SettingsKey] 
SET KeyValue = NULL 
WHERE KeyName = N'CMSLicenseKey';

-- Set the Instance Name
UPDATE [CMS_SettingsKey] 
SET KeyValue = N'Goldfinch.me Local'
WHERE KeyName = N'CMSInstanceFriendlyName' ;

-- Enable Continuous Integration
UPDATE [CMS_SettingsKey] 
SET KeyValue = N'True' 
WHERE KeyName = N'CMSEnableCI';

COMMIT TRANSACTION;