# my-tesla
Personal project to interact with my Tesla

I have terrible memory and have forgotten to plug in my car to charge over night several times. Usually it's not a big deal at all (first-world problems), but it's still a minor annoyance. 
So I decided to create a small Windows service to remind me to plug in to charge. 

Basically the service takes the following steps:
 * Check if it's time to charge (configured to >= 9 PM for me)
 * Check if vehicle is at home (within 50 meters of my home's lat/long coordinates)
 * Check if vehicle is currently disconnected from charger
 * Send email to remind me to plug in

I found this site very handy for generating C# models based on JSON results, and recommend its use whenever JSON structure changes - http://json2csharp.com/
