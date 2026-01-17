class SemanticVersion {
    [int] $Major
    [int] $Minor
    [int] $Patch
    [string] $Prerelease

    SemanticVersion ([int] $major, [int] $minor, [int] $patch, [string] $prerelease = $null) {
        $this.Major = $major
        $this.Minor = $minor
        $this.Patch = $patch
        $this.Prerelease = $prerelease
    }

    static [SemanticVersion] TryParse([string] $versionString) {
        if ($versionString -match '^(?<major>\d+)\.(?<minor>\d+)(?:\.(?<patch>\d+)?)(-(?<prerelease>[0-9A-Za-z.-]+))?$') {
            $parsedMajor = [int]$matches['major']
            $parsedMinor = [int]$matches['minor']
            $parsedPatch = if ($matches['patch']) { [int]$matches['patch'] } else { 0 }
            $parsedPrerelease = $matches['prerelease']

            return [SemanticVersion]::new($parsedMajor, $parsedMinor, $parsedPatch, $parsedPrerelease)
        } else {
            return $null
        }
    }

    [string] ToString() {
        if ($this.Prerelease) {
            return "$($this.Major).$($this.Minor).$($this.Patch)-$($this.Prerelease)"
        } else {
            return "$($this.Major).$($this.Minor).$($this.Patch)"
        }
    }
}