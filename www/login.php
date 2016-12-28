<?php
include 'config.php';
include 'accounts_class.php';
if (!isset($_POST["commfunction"]) || empty($_POST["commfunction"])) 
{
	echo "failed:\n";
} 
elseif (!isset($_POST["version"]) || empty($_POST["version"]) || $_POST["version"] != $game_version) 
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
			case "removesession":
			if(!$account->RemoveSession())
			{
				echo "failed:\n";
				exit;
			}
			break;
		}
	}
	else if($game_autologin && $account->Create())
	{
		echo "authresponse:\ntrue\n".$account->Session()."\n".$account->AccessLevel()."\n".$account->Id()."\n".$game_servers;
		exit;	
	}
	echo "authresponse:\nfalse\n";
}
?>