# phpBox, your PHP mate! #


Table of contents


## Research ##
At this point of development i decided to create a second version of phpBox with cleaner code, cleaner design and better performance. One important thing is to make the output faster. For this i am looking for someone who can help me to create an element for really fast output.

## About ##

### History ###
phpBox is a small and lightweight everyday-assistant if it comes to PHP development. If you are familiar with PHP, you know there is neither a GUI nor any simple way to execute your script just for two lines of code. The idea for such a small and simple helper started somewhere around 2008 here at GEDAK. I, **Lars**, write PHP scripts for nearly every task. Even if there are 200 files to be renamed in a special, not common definable/renameable way, I pop off an UltraEdit instance and write some lines of PHP to solve the task. In 2008, **Armin**, a work mate of mine, wrote me PHPrun. It solved my problems very well but left some space for improvement. Sadly he left the company to reach out for more (and less coding ;-) )

### Present ###
Luckily, **Viktor**, an apprentice here at GEDAK and very smart guy, picked up the idea during an internal workshop and started a complete rewrite with the goal to push the great PHPrun to the next level. phpBox got born in less than 7 days. Fully backward compliant to PHPrun, faster, more precise, more beautiful and a bunch of improvements.

#### The goal of phpBox is simple: ####
  * Most easy handling
  * Does not bother the user
  * Acts contextual
  * No configuration
  * Common shortcuts like F5 and ESC
  * Makes execution of PHP scripts an eye candy in cases where scripts have just to work
  * ...to boost your productivity and fun while coding PHP!

We have still a lot of ideas for improvements but be sure: We will never blow up the easiness of the user experience it has already archieved. We promise!

Lars Echterhoff, in behalf of Viktor Machnik and GEDAK.

Visit us www.gedak.de

Why do we share our work as a profit orientated IT company? Because we want to evolve together with the future. **We want to show you honestly what you have to expect from us: Straightforward thoughts and working solutions!**

## Changelog ##

  * 2012/03/14: v1.3.2 got a build in web control and several bugfixes.
  * Inbetween: Sorry for the lag of updates.
  * 2012/01/27: Initial release v1.1. Hooray!

## Quickstart ##

Download a copy of php binaries from http://windows.php.net/download/ and place a copy of phpBox.exe within the php root folder. Double click phpBox and start executing php scripts.

If you use UltraEdit or Notepad++, you would like to prefer to execute phpBox from their. The configuration is very similar.

### Run from editor ###

#### UltraEdit ####
  1. Place phpBox.exe within your local php copy.
  1. Choose "Extras" -> "Tools-Configuration"
  1. Name it ```dos
phpBox```
  1. The command line should look like: ```dos
c:\php5\phpbox.exe -r php.exe -s "%F" -p "variablestring"```
  1. The working directory should contain ```dos
%P```
  1. Thats it.

#### Notepad++ ####
  1. btc.

#### Paramter ####
  1. -r | -runtime Specifies php.exe
  1. -s | -script PHP script you want to run
  1. -p | -parameter Enclosed by quotes, specify here a url-encoded parameter string.
  1. -h | -help Guess what? ;-)

## Syntax ##

**The current syntax is deprecated with the date of release.**
This very first release is a legacy release to seamless replace the former PHPrun with phpBox. The new syntax will be announced in an upcoming version together with a lightweight and flexible php class.
We will not break the legacy syntax.

Each command has to be fired seperate by an echo and end with a line end
```php

echo "{STATUS|Test text}\n";```
This will result in an output of "Test text" inside the status bar of phpBox.

```php

echo "Common output string\n";```
This results in a common output inside the phpBox output log.

## F.A.Q ##

  * Q: I start phpBox.exe and it says `"php.exe not found!"`. Whats wrong?
> > A: You have probably neither placed phpBox.exe into the php directory nor specified the path to your php executable. If you tried to start phpBox from and editor, please review the command line. Otherwise try to copy it into your php folder and start it from there.

## Screenshots ##
### Version 1.3.2 ###
![http://cl.ly/1c2t3W2g0E3A0I2b2R3R/Image%202012-04-18%20at%2010.57.05%20AM.png](http://cl.ly/1c2t3W2g0E3A0I2b2R3R/Image%202012-04-18%20at%2010.57.05%20AM.png)

### Version 1.1 ###
![http://dl.dropbox.com/u/134085/code.google.com/phpBox/screenshot_v1.1.png](http://dl.dropbox.com/u/134085/code.google.com/phpBox/screenshot_v1.1.png)