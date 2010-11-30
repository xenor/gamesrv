#!/usr/bin/env php
<?php
$host = "localhost";
$port = 3000;
$name = "xenor";
$items = array(
	1 => (object) array(
		"name" => "Raumkraut",
		"desc" => "Berauscht dich!",
	),
	2 => (object) array(
		"name" => "Testkeks",
		"desc" => "TESTKEKS!!!!",
	),
);
date_default_timezone_set("Europe/Berlin");
$sock = fsockopen($host,$port);
if(!$sock) die();
fputs($sock,"LOGIN;xenor;0;lalaftw#!");
$online		= true;
$ready		= false;
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
				$item_id	= trim($cmd[2]);
				$vnum		= trim($cmd[3]);
				echo		$item_id . ":\t" . $items[$vnum]->name . ":\t" . $items[$vnum]->desc . "\n";
			}
			//echo date("r",time()).": ".trim($str)."\r\n";
		}
	}
} while($online == true);
fputs($sock,"QUIT");
echo "Connection Lost.\r\n";
?>
