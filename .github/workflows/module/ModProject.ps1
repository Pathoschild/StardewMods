class ModProject
{
    [ValidateNotNull()] [System.IO.FileInfo] $File
    [ValidateNotNull()] [Xml] $FileXml
    [ValidateNotNullOrEmpty()] [string] $RawVersion
    [SemanticVersion] $ParsedVersion
    [bool] $EnableModZip

    ModProject ([System.IO.FileInfo] $file, [Xml] $xml, [string] $version, [bool] $enableModZip) {
        $this.File = $file
        $this.FileXml = $xml
        $this.RawVersion = $version
        $this.ParsedVersion = [SemanticVersion]::TryParse($version)
        $this.EnableModZip = $enableModZip
    }

    [string] ToString() {
        return "$($this.File.BaseName) v$($this.ParsedVersion ?? $this.RawVersion)"
    }
}
