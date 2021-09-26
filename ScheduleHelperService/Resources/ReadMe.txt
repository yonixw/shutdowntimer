Minimal example config
===================================
programs:
- &myexe1
  args:
  path: notepad.exe
schedule:
  weekday:
    pattern: '* 1-59/2 * * * ? *' # Cron expression
    targets:
    - *myexe1
    timezone: Greenwich Standard Time # empty for UTC

TimeZones:
===================================
Choose a full qualified name based on operation system:
* Mac OS: https://archive.is/5Ak2e
* Windows: https://archive.is/6CZ2V


Cron expression help (from: https://archive.ph/J37Js):
===================================
* (“all values”)
? (“no specific value”)
	- either day of month or day of week (can't specify both)
/ (used to specify increments. For example, “0/15” = 0,15,30,45)
L - Last day of month or week
W - weekdays (Mon-Fri)
# - 6#3 in the day of week field means the third Friday 
	- (day 6 is Friday; #3 is the 3rd Friday in the month). 

Field Name \ Allowed Values \ Allowed Special Characters
===================================
Seconds         0-59                 , - * /
Minutes         0-59                 , - * /
Hours           0-23                 , - * /
Day-of-month    1-31                 , - * ? / L W
Month           1-12 or JAN-DEC      , - * /
Day-of-Week     1-7 or SUN-SAT       , - * ? / L #
Year (Optional) empty, 1970-2199     , - * /
===================================