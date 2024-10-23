#!/bin/bash
echo -ne "\033]0;Migration Manager\007"

script_dir=$(dirname "$0")

project_relative_path="src/Common/Common.Persistence/Common.Persistence.csproj"
startup_project_relative_path="src/API/API.csproj"

project_path="$script_dir/$project_relative_path"
startup_project_path="$script_dir/$startup_project_relative_path"

add_migration() {
  timestamp=$(date +"%Y%m%d%H%M%S")
  random_name="Migration${timestamp}"

  dotnet ef migrations add $random_name -o Migrations -n Common.Persistence.Migrations -c HVODbContext -p $project_path -s $startup_project_path
}

remove_migration() {
  dotnet ef migrations remove -c HVODbContext -p $project_path -s $startup_project_path
}

run_project() {
  # Ensure the script uses the directory where the .sh file is located
  script_dir=$(dirname "$0")

  # Define the relative paths to the projects
  api_project_path="src/API/API.csproj"
  portal_path="src/Portal"

  # Construct the absolute paths
  api_project_full_path="$script_dir/$api_project_path"
  portal_full_path="$script_dir/$portal_path"

  # Check if the .csproj file exists
  if [ ! -f "$api_project_full_path" ]; then
    echo "Error: The project file for dotnet run does not exist at $api_project_full_path"
    exit 1
  fi

  # Check if the portal directory exists
  if [ ! -d "$portal_full_path" ]; then
    echo "Error: The directory for yarn run dev does not exist at $portal_full_path"
    exit 1
  fi

  # Open a new Terminal tab or window for the dotnet run command
  (
    cd "$script_dir/src/API" || exit
    echo "Running dotnet run..."
    dotnet run --project "$api_project_full_path"
  ) &

  # Open a new Terminal tab or window for the yarn run dev command
  (
    cd "$portal_full_path" || exit
    echo "Running yarn run dev..."
    yarn run dev
  ) &

  # Wait for all background jobs to finish
  wait
}

show_menu() {
  echo "1) Add migration"
  echo "2) Remove migration"
  echo "3) Run project"
  echo "4) Exit"
  read -p "Choose an option: " choice

  case $choice in
  1)
    add_migration
    ;;
  2)
    remove_migration
    ;;
  3)
    run_project
    ;;
  4)
    exit 0
    ;;
  *)
    echo "Invalid option"
    show_menu
    ;;
  esac
}

chmod +x "$0"

while true; do
  show_menu
done
