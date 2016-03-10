PasswordManager
======

PasswordManager is a tiny command line tool for generating passwords using a deterministic key derivation algorithm. 


Goals
-----
I coded it because i am not able to create really strong passwords and keep them in my brain anymore. With PasswordManager you only
have to remember a master key and that's all, you can give a different very-strong password for each service that you use online.

+ Tested with .NET  _YES_
+ Tested with Mono  _NO_ 

How to use?
-----------

![screencast gif](http://i.imgur.com/NzqJLc2.gif)

    
    C:\>pwd --help
    Usage: spm
    Simple Password Manager for Windows (c) 2016 - Lucas Ontivero <lucasontivero@gmail.com>.
    
    Options:
      -h, -?, --help         Shows this help and exit
      -l, --ls               Lists all the credentials in the repository
      -a, --add=CREDENTIAL   Adds a new CREDENTIAL to the repository
      -v, --view=CREDENTIAL  Displays the CREDENTIAL details
      --init                 Initializes the repository
    

- **pwd --init** initializes the credentials local repository. To do this it creates a new EC private key (256 bits key) and requests the user to enter the master key that will be used for encrypting the private master key. In windows the repository is allocated in the "%AppData%\Simple Password Manager" folder.
- **pwd** displays the list of pass_ids in the repository.
- **pwd pass_id** returns the password for pass_id in the clipboard
- **pwd -v pass_id** returns the password for pass_id and display it in the console
- **pwd --add pass_id** generates and adds a new password to the local repository. For example: <pwd --add lontivero@trello.com> generates a new password for the trello website.

## Usage Session ##

    C:\> pwd --init
    Password:
    ....    
    ....
    ....

    C:\> pwd 
    Password:
    e650b3 20 24/04/2016 11:47:17 p.m. +00:00 localadmin@vmwin2012.company.com
    f48d6e 20 02/06/2016 03:02:10 a.m. +00:00 lontivero@github.com
    9162fc 20 01/01/2020 02:28:01 a.m. +00:00 lontivero@imgur.com
    da1a28 20 20/04/2016 10:25:02 p.m. +00:00 lucas.ontivero@email.com
    a75b1e 20 24/04/2016 12:19:23 p.m. +00:00 root@ubuntuserver.company.com
    
    C:\> pwd -v imgur
    Password:
    Wr9DbrLsCadysWlVmhyt
    
    C:\> pwd -a lontivero@reddit.com
    Password:
    
    C:\> pwd reddit
    Password:
    
    C:\> pwd -a another@reddit.com
    Password:
    
    C:\> pwd reddit
    Password:
    f4916c 20 24/04/2016 03:02:10 a.m. +00:00 lontivero@reddit.com
    2f8d6e 20 24/04/2016 03:03:01 a.m. +00:00 another@reddit.com



Development
-----------
PasswordManager was developed by [Lucas Ontivero](http://geeks.ms/blogs/lontivero) ([@lontivero](http://twitter.com/lontivero)) 
in a few hours what means there are many things to improve. 
You are very welcome to contribute code. You can send code both as a patch or a GitHub pull request. 

Build Status
------------
[![Build status](https://ci.appveyor.com/api/projects/status/ld432ycr3nycf6k8?svg=true)](https://ci.appveyor.com/project/lontivero/passwordmanager)

### Version 0.0.2

##If this is useful for you invite me a beer

Bitcoin Address: 15fdF4xeZBZMqj8ybrrW7L392gZbx4sCXH
