$apps = Read-Host -Prompt 'Input app names separated by "," e.g. "ConsoleApp, MinimalApi, WindowsService, Blazor"'
$ostype = $(If (($(docker version) -split [Environment]::NewLine)[18].ToLower() -Match "windows") {"Windows"} Else {"Linux"})
$osnames = switch ($ostype) 
{
    "Windows" {"Nano", "ServerCore"}
    "Linux" {"Alpine"}
} 
$ossymbol = switch ($ostype) 
{
    "Windows" {"win-x64"}
    "Linux" {"linux-x64"}
} 
$cases = "DotNetAndPowerShell", "SelfContained", "Slim"

foreach ($app in $apps)
{
    foreach ($osname in $osnames)
    {
        foreach ($case in $cases)
        {
            $imagename = "$($osname.ToLower())-dotnet-$($app.ToLower()):$($case.ToLower())"
            $containername = "$($osname.ToLower())-dotnet-$($app.ToLower())-$($case.ToLower())"
            $projectpath = "$app/$ostype/$osname/$case"

            if ($case -eq "SelfContained")
            {
                dotnet publish $projectpath/$app.csproj -c release -r $ossymbol --self-contained
            }

            docker image build -t $imagename -f $projectpath/Dockerfile ./$projectpath
            docker container run -d --name $containername $imagename
        }
    }
}