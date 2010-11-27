#!/usr/bin/env php
<?php
$host = "localhost";
$port = 3000;
if(isset($argv[1]))
{
	$name = $argv[1];
}
else
{
	$name = "xenor";
}
$items = array(1 => "Raumkraut",2 => "Testkeks");
date_default_timezone_set("Europe/Berlin");
$sock = fsockopen($host,$port);
if(!$sock) die();
fputs($sock,"LOGIN;xenor;0;lalaftw#!");
$online = true;
$ready = false;
do
{
	if(feof($sock) || !$sock) $online = false;
	else
	{
		$str = fgets($sock);
		$cmd = explode(';',$str);
		if(trim($str) != "")
		{
			if($cmd[0] == "ITEM" && $cmd[1] == "OK")
			{
				$item_id = trim($cmd[2]);
				var_dump($item_id);
				$vnum = trim($cmd[3]);
				var_dump($vnum);
				echo $items[$vnum] . "\n";
			}
			echo date("r",time()).": ".trim($str)."\r\n";
		}
	}
} while($online == true);
fputs($sock,"QUIT");
echo "Connection Lost.\r\n";
?>
