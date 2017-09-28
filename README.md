# Legends of Equestria Private Server
Tested on Windows 10 with 22.09.2017 client release.
# Content
Server only has testing content (npcs, dialogues, monsters) and no any content from official server.
# How to use
You need php+mysql web server like this <a href="http://sourceforge.net/projects/wampserver/">Wamp Server</a>
<ol>
<li>Install <a href="https://www.microsoft.com/net/download/core#/runtime">.Net Core Runtime 2.0</a>
<li>Clone or download this repo</li>
<li>Execute <i>legends_of_equestria.sql</i> on mysql server</li>
<li>Navigate to &lt;Repo Dir&gt;/Server</li>
<li>Execute in console <i>dotnet restore</i></li>
<li>Than <i>dotnet run</i> for generating default configs file</li>
<li>For local using:<ul>
<li>Copy <i>connection_s.json</i> to you Legends of Equestria game folder</i></li>
<li>Replace mysql user and password in <i>loe_server.cfg</i> and <i>config.php</i> by your own</li></ul></li>
<li>Copy files from <i>www</i> folder to web server <i>www</i> folder</li>
<li>Execute <i>dotnet run</i> again and wait full loading them type command: 
<b>user create</b> &lt;login&gt; &lt;password&gt; &lt;access&gt;
<br>access:<ul>
<li>Player = 1</li>
<li>TeamMember = 20</li>
<li>Implementer = 25</li>
<li>Moderator = 30</li>
<li>Admin = 255</li></ul>
type help for full commands list</li>
<li>?????</li>
<li>PROFIT!</li></ol>
