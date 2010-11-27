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
date_default_timezone_set("Europe/Berlin");
$sock = fsockopen($host,$port);
if(!$sock) die();
fputs($sock,"LOG;" . $name);
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
            if(trim($cmd[0]) == "PING") fputs($sock,"PONG");
            if(trim($cmd[0]) == "LOG")
            {
                if(trim($cmd[1]) == "OK")
                {
                    $ready = true;
                }
                else
                {
                    $online = false;
                }
            }
            if($ready == true)
            {
                echo date("r",time()).": ".trim($str)."\r\n";
            }
        }
    }
} while($online == true);
fputs($sock,"QUIT");
echo "Connection Lost.\r\n";
?>
