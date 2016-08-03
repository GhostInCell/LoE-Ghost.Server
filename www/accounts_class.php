<?php
class AccountsMgr 
{
	private $db_conn;
	private $db_base;
	private $db_stat;
	private $db_loe_accounts;
	private $game_login;
	private $game_account;
	private $game_passhash;
	private $game_account_id;
	private $game_account_data;
	private $game_account_access;
	private $game_account_session;
	function __construct() 
	{
		require 'config.php';
		$this->db_loe_accounts = $db_loe_accounts;
		$this->db_conn = new mysqli($db_host, $db_user, $db_pass, $db_loe);
		$this->db_stat = !$this->db_conn->connect_errno;
		if($this->db_stat && isset($_POST["username"]) && !empty($_POST["username"]))
		{
			$this->game_login = $this->db_conn->real_escape_string($_POST["username"]);
			$this->game_account = $this->db_conn->query("SELECT id, phash, access, session FROM $db_loe_accounts WHERE login='$this->game_login'");
			if ($this->game_account) 
			{
				$this->game_account_data = $this->game_account->fetch_assoc();
				$this->game_account_id = $this->game_account_data["id"];
				$this->game_account_access = $this->game_account_data["access"];
				$this->game_account_session = $this->game_account_data["session"];
				$this->game_account->free();
			}
		}
	}
	function __destruct() 
	{
		if($this->db_stat)
		{
			$this->db_conn->close();
		}
	}
	function IsExist() 
	{
		if($this->game_account_data)
		{
			return true;
		}
		return false;
	}
	function Id() 
	{
		return $this->game_account_id;
	}
	function Session() 
	{
		return $this->game_account_session;
	}
	function AccessLevel() 
	{
		return $this->game_account_access;
	}
	function Login() 
	{
		if($this->game_account_data)
		{
			if (isset($_POST["passhash"]) && !empty($_POST["passhash"]) && strcmp($_POST["passhash"], $this->game_account_data["phash"]) == 0) 
			{
				$time = date('Y-m-d H:i:s', time());
				$this->game_account_session = base64_encode(hash("tiger192,3", "Celestia".$time."Luna".$this->game_login."~zbKG5tGWqv"));
				if($this->db_conn->query("UPDATE $this->db_loe_accounts SET session='$this->game_account_session', time='$time' WHERE id=$this->game_account_id"))
				{		
					return true;
				}
			}
		}
		return false;
	}
	function Create() 
	{
		if(!$this->game_account_data)
		{
			if (isset($_POST["passhash"]) && !empty($_POST["passhash"])) 
			{
				$time = date('Y-m-d H:i:s', time());
				$this->game_account_access = 1;
				$this->game_passhash = $this->db_conn->real_escape_string($_POST["passhash"]);
				$this->game_account_session = base64_encode(hash("tiger192,3", "Celestia".$time."Luna".$this->game_login."~zbKG5tGWqv"));
				if($this->db_conn->query("INSERT INTO $this->db_loe_accounts (login, phash, access, session, time) VALUES ('$this->game_login', '$this->game_passhash', $this->game_account_access, '$this->game_account_session', '$time')"))
				{	
					$this->game_account_id = $this->db_conn->insert_id;
					return true;
				}
			}
		}
		return false;
	}
	function RemoveSession() 
	{
		if($this->game_account_session)
		{
			if($this->db_conn->query("UPDATE $this->db_loe_accounts SET session=NULL WHERE id=$this->game_account_id"))
			{
				return true;
			}
		}
		return false;
	}
} 
?>