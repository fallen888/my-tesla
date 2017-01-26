# my-tesla
Personal project to interact with my Tesla

I have a terrible memory and have forgotten to plug in my car to charge over night several times. Usually it's not a big deal at all (first-world problems), but it's still a minor annoyance. 
So I decided to create a small Windows service (runs on my media server at home) to remind me to plug in to charge. 

Caution: This is a very basic application with some dirty, ugly code. Future enhancements should definitely include some code clean-up, error logging, etc.

Basically the service takes the following steps:
 * Check if it's time to charge (configured to >= 9 PM for me)
 * Check if vehicle is at home (within 50 meters of my home's lat/long coordinates)
 * Check if vehicle is currently disconnected from charger
 * Send SMS to remind me to plug in

Sample SMS messages:

![SMS message example](https://github.com/fallen888/my-tesla/blob/master/images/sms_screenshot.png?raw=true "SMS message example")

I'm leveraging a free SMS service, just for my personal use - http://textbelt.com/

I also found this site very handy for generating C# models based on JSON results, and recommend its use whenever JSON structure changes - http://json2csharp.com/
