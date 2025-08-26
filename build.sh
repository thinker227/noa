#!/bin/sh

# Fail on error
set -e

if [ $# -ne 1 ]; then
    echo "Usage: $0 <output-path>"
    exit 1
fi

# Expand path
output="$(realpath $1)"

# Check for / as a safety precaution
if [ $output == "/" ]; then
    echo "no"
    exit 1
fi

dir_exists=false
if [ -d $output ]; then
    dir_exists=true
fi

# Check if output directory exists, is not empty, and does not have a file named .noaoutputdir
if [ $dir_exists = true ] && [ ! -z "$( ls -A $output )" ] && [ ! -f "$output/.noaoutputdir" ]; then
    echo -e "Directory $output is not empty and not marked as an existing output directory.\nPlease remove or empty it before proceeding."
    exit 1
fi

# Remove existing directory
if [ $dir_exists = true ]; then
    echo "Removing existing directory..."
    rm -rd $output
fi

echo "Building to $output"

# Create directory and .noaoutputdir file
mkdir $output
echo "metadata for build.sh" > "$output/.noaoutputdir"

# Build compiler
echo -e "\n  Building compiler\n"
dotnet build src/cli -c release -o $output --nologo
mv $output/Noa.Cli $output/noa

# Build runtime
echo -e "\n  Building runtime\n"
cargo b -r --target-dir $output/rust
cp $output/rust/release/noa_runtime_cli $output/noa_runtime
rm $output/rust -rd

echo -e "\nFinished!"
echo -e "Main executables produced: noa, noa_runtime\n"
echo "Set up \$PATH: echo -e '\n# Noa\nexport PATH=\$PATH:$output' >> ~/.bashrc && . ~/.bashrc"
echo ""

exit 0
