# EETWrapper
Czech EET Wrapper in .NET

Completely using WCF features. No HTML replace bullshit. :-)

**UPDATE 2016-11-16**
- After huge fight with WCF I've got a working solution again (still a lot of clean up work to do)
- Guys designing EET change the web service to enhance SOAP security, which caused my code suddenly to stop to work
- Had to give up on OOTB WCF solution and create own WCF MessageInspector

**UPDATE 2016-10-21**
- Basic code in EETProvider
  - Logging for the moment done with an event, we will see if this is a good approach
- Almost done with the mapping of EETData to EET message objects
- Preparations for multilanguage support with resources
