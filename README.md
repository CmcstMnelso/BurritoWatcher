# BurritoWatcher
Get them burritos

This does exactly what it says and watches for burritos flying by during the playoffs, and allows for sharing with friends.

Requirements:
  1. A twitter bearer token from a project on twitter
  2. Windows machine (IronOCR requirement, ADB/.bats configured for windows environment)
  3. A modern android phone connected via USB with USB-debugging enabled (for sending text messages)

Setup
  Please create a secrets.json file in the root of the application directory that includes the twitter bearer token and any relevant contacts. (Example below)
  
  --settings.json
  
  {
    "TwitterBearerToken":"GET BEARER TOKEN FROM TWITTER PROJECT CREDENTIALS SITE",
	  "Contacts":[{"Name":"Test Person","Number":1234567890,"Type":"Android"}]
  }
