# Errors #
1. Database issues
  * 'update-database' doesn't work i get "No migration ... information".
  * Table 'account' already exists.
  * Wrong password, but i set it properly in database.

2. Nu-Get issues
  * Project 'OpenNos.{name}' failed to build.
 
3. Connection issues
  * Can login but cant join channel.
  * On login nothing happens.
  * Wrong password.
  
4. World Issues
  * Monsters don't move.
  * Recipes don't work.
  * Theres no portals.
  * Monsters miss some stats.

# Fixes #
1. Database issues
  1. Change Default project in Package Manager Console to OpenNos.EF.MySQL.
  2. Before using command update-database, make sure you dropped the opennos database in SSMS.
  3. Make sure you're using latest version of client with proper ip set in world settings. (Look also on this cool [project](https://github.com/genyx/OpenNosClientLauncher)).

2. Nu-Get issues
  1. Make sure, that you used restore nu-get packages before trying to compile the project. As well make sure you have all dependencies like .NET Framework 4.7.1 Targeting Pack installed, in case of precompiled release make sure you have .NET Framework 4.7.1 installed.

3. Connection issues
  1. Often caused by wrong client crypto make sure that you use latest version of client.
  2. Verify you utilize the correct port of your client. If not, you installed something wrong, check if you have disabled any programs working on ports 1337, 6969 and your login ports(depends on your language settings).
  3. Verify that your password is hashed in sha512 and that your launcher(made it yourself) is done with the most recent nostaleClientX.exe.
  
4. World Issues
  1. Parse mv packets.
  2. Parse each recipe by clicking on them in game.
  3. When you gather the packets you need to go through each portal once.
  4. You need to right click on each different monster once soo you gather their data.

# Unexpected behavior #
- If project behaves unstable/improperly or something is off try to open a new issue explaining in details your problem with emulator, also make sure the database data is proper.
- You can also contact us on [our discord server](https://discordapp.com/invite/N8eqPUh).
