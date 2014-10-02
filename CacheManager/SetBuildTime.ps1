 param (
    [string]$SolutionDir = $(throw "$SolutionDir is required.")
 )

$xmlFile = $SolutionDir + "TacklR.CacheManager\Properties\Resources.resx"
$xml = NEW-OBJECT XML
$xml.load($xmlFile);
$buildTime = $xml.selectSingleNode("root/data[@name=""BuildTime""]/value")
if ($buildTime -ne $null) {
    $buildTime.InnerText = ((get-date).ToUniversalTime()).ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
    $xml.save($xmlFile);
}
