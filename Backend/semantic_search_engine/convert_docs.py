import os
import re
from bs4 import BeautifulSoup

# Define paths
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
INPUT_DIR = os.path.join(BASE_DIR, "unity_docs")
OUTPUT_DIR = os.path.join(BASE_DIR, "data")

os.makedirs(OUTPUT_DIR, exist_ok=True)

def clean_html_file(file_path):
    with open(file_path, "r", encoding="utf-8") as f:
        html_content = f.read()

    soup = BeautifulSoup(html_content, "html.parser")

    # Remove script and style elements
    for element in soup(["script", "style"]):
        element.decompose()

    # Specifically remove the inline glossary tooltips text to prevent keyword noise
    for tooltip in soup.find_all("span", class_="tooltiptext"):
        tooltip.decompose()

    # Extract Page Title
    title_tag = soup.find("title")
    title = title_tag.get_text() if title_tag else ""
    title = title.replace("Unity - Manual: ", "").replace("Unity - Scripting API: ", "").strip()
    if not title:
        # Fallback to H1
        h1_tag = soup.find("h1")
        title = h1_tag.get_text().strip() if h1_tag else os.path.basename(file_path)

    # Extract Breadcrumbs (structural hierarchy)
    breadcrumbs_div = soup.find("div", class_="breadcrumbs")
    breadcrumbs = ""
    if breadcrumbs_div:
        li_elements = breadcrumbs_div.find_all("li")
        breadcrumbs_list = [li.get_text().strip() for li in li_elements if li.get_text().strip()]
        breadcrumbs = " > ".join(breadcrumbs_list)

    # Extract main body text
    # Unity docs usually place main manual content inside <div class="section">, <div class="content"> or <div class="content-block">
    content_div = soup.find("div", class_="section") or soup.find("div", class_="content") or soup.find("div", class_="content-block")
    
    if content_div:
        # Get text from the specific content div
        text = content_div.get_text(separator="\n")
    else:
        # Fallback to body or entire soup
        body = soup.find("body")
        text = body.get_text(separator="\n") if body else soup.get_text(separator="\n")

    # Clean up whitespace and consecutive newlines
    lines = [line.strip() for line in text.splitlines()]
    # Remove empty lines and headers/footers crumbs if present
    cleaned_lines = []
    for line in lines:
        if not line:
            continue
        # Skip breadcrumbs or next/prev text if it accidentally gets extracted
        if line.startswith("PackagesList") or line.startswith("FeatureSets"):
            continue
        cleaned_lines.append(line)
        
    cleaned_text = "\n".join(cleaned_lines)

    # Format structured output text file
    structured_content = f"Title: {title}\nPath: {breadcrumbs}\n\n{cleaned_text}"
    return structured_content

def main():
    print(f"Reading HTML files from: {INPUT_DIR}")
    print(f"Saving converted text files to: {OUTPUT_DIR}")

    if not os.path.exists(INPUT_DIR):
        print(f"Error: {INPUT_DIR} does not exist!")
        return

    html_files = [f for f in os.listdir(INPUT_DIR) if f.endswith(".html") or f.endswith(".htm")]
    total_files = len(html_files)
    print(f"Found {total_files} HTML files to convert.")

    converted_count = 0
    for idx, filename in enumerate(html_files, 1):
        input_path = os.path.join(INPUT_DIR, filename)
        output_filename = os.path.splitext(filename)[0] + ".txt"
        output_path = os.path.join(OUTPUT_DIR, output_filename)

        try:
            structured_text = clean_html_file(input_path)
            with open(output_path, "w", encoding="utf-8") as f:
                f.write(structured_text)
            
            converted_count += 1
            if idx % 100 == 0 or idx == total_files:
                print(f"Converted {idx}/{total_files} files...")
        except Exception as e:
            print(f"Failed to convert {filename}: {str(e)}")

    print(f"Conversion completed! Successfully converted {converted_count}/{total_files} files.")

if __name__ == "__main__":
    main()
