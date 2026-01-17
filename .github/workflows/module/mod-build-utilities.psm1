. $PSScriptRoot/SemanticVersion.ps1
. $PSScriptRoot/ModProject.ps1

<#
.SYNOPSIS
Get all C# mod project files found in a given path.

.PARAMETER Path
The root path to search for project files.

.PARAMETER OnlyModZipped
Whether to only select mod projects which create a release zip.
#>
function Get-ModProjects {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory=$false)] [string] $Path = ".",
        [Parameter(Mandatory=$false)] [switch] $OnlyModZipped = $false
    )

    process {
        $projects = Get-ChildItem -Path $Path -Recurse -Filter *.csproj
        Select-ModProjects -Projects $projects -OnlyModZipped:$OnlyModZipped
    }
}

<#
.SYNOPSIS
Given a list of C# project files, returns metadata parsed from the project file relevant to
automated build pipelines. This doesn't support inherited build properties.

.PARAMETER Projects
The project files to read.

.PARAMETER OnlyModZipped
Whether to only select mod projects which create a release zip.
#>
function Select-ModProjects {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)] [System.IO.FileInfo[]] $Projects,
        [Parameter(Mandatory=$false)] [switch] $OnlyModZipped = $false
    )

    process {
        foreach ($file in $Projects) {
            $xml = [xml](Get-Content $file.FullName)
            $versionNode = Select-Xml -Xml $xml -XPath '//Version' | Select-Object -Last 1
            $enableModZipNode = Select-Xml -Xml $xml -XPath '//EnableModZip' | Select-Object -Last 1

            $version = $versionNode ? $versionNode.Node.InnerText : $null
            $enableModZip = if ($enableModZipNode) { [bool]::Parse($enableModZipNode.Node.InnerText) } else { $true }

            if ($OnlyModZipped -and -not $enableModZip) {
                continue
            }

            [ModProject]::new($file, $xml, $version, $enableModZip)
        }
    }
}

<#
.SYNOPSIS
Increment the versions of C# mod project files to a timestamped prerelease version.

.PARAMETER ProjectPaths
The paths to the project files to set.
#>
function Set-PrereleaseModVersions {
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)] $Projects # note: can't set type to [ModProject[]] because the parameter is bound before the separate type is loaded from the separate file
    )

    process {
        $timestamp = (Get-Date).ToUniversalTime().ToString('yyyyMMddHHmm')

        foreach ($project in $Projects) {
            if (!($project -is [ModProject])) {
                throw "Expected a ModProject, got $($project.GetType().FullName)"
            }

            # validate version
            if (!$project.RawVersion) {
                Write-Warning "Skipped $($project): no <Version> found."
                continue
            }
            if (!$project.ParsedVersion) {
                Write-Warning "Skipped $($project): version is set to non-semantic value '$($project.RawVersion)'."
                continue;
            }

            # build new version
            $oldVersion = $project.ParsedVersion
            $newPatch = $oldVersion.Patch
            if (!$oldVersion.Prerelease) {
                $newPatch += 1
            }
            $newVersion = "$($oldVersion.Major).$($oldVersion.Minor).$newPatch-alpha.$timestamp"

            # update project
            $project.FileXml.SelectNodes( '//Version' ) | Select-Object -Last 1 | ForEach-Object { $_.InnerText = $newVersion }
            $project.FileXml.Save($project.File.FullName)

            Write-Host "Updated $($project) -> '$newVersion'"
        }
    }
}

Export-ModuleMember -Function Get-ModProjects, Select-ModProjects, Set-PrereleaseModVersions
