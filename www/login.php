<?php
include 'config.php';
include 'accounts_class.php';
if (empty($_POST["commfunction"])) 
{
	echo "failed:\n";
} 
elseif ($_POST["version"] !== $game_version) 
{
    echo "versionresponse:\n".$game_version;
} 
else 
{
	$account = new AccountsMgr();
	if ($account->IsExist()) 
	{
		switch ($_POST["commfunction"]) 
		{
			case "login":
				if($account->Login())
				{
					echo "authresponse:\ntrue\n".$account->Session()."\n".$account->AccessLevel()."\n".$account->Id()."\n".$game_servers;
					exit;
				}
				break;
			case "login_21":
				$_POST["passhash"] = sha1(strtolower($_POST["username"]).$_POST["passwrd"]);
				if($account->Login())
				{
					echo "authresponse:\ntrue\n".$account->Session()."\n".$account->AccessLevel()."\n".$account->Id()."\n".$game_servers;
					exit;
				}
				break;
			case "ucheck":
				if($account->Valid())
				{
					echo "ucheckresponse:\nSuccess\n".$account->AccessLevel();
					exit;
				}
				echo "ucheckresponse:\nFalse\n0";
				exit;
			case "removesession":
				if(!$account->RemoveSession())
				{
					echo "failed:\n";
					exit;
				}
				break;
		}
	}
	else if($game_autologin)
	{
		if ($_POST["commfunction"] === "login_21")
		{
			$_POST["passhash"] = sha1(strtolower($_POST["username"]).$_POST["passwrd"]);
		}
		if($account->Create())
		{
			echo "authresponse:\ntrue\n".$account->Session()."\n".$account->AccessLevel()."\n".$account->Id()."\n".$game_servers;		
			exit;
		}
	}
	echo "authresponse:\nfalse\n";
}
?>