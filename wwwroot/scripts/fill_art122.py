"""
fill_art122.py
Called by Art122PdfService.RunPythonAsync().
Usage: python fill_art122.py <template_path> <output_path>
Fields JSON is passed via the ART122_FIELDS environment variable.

Install once:  pip install pypdf
"""
import sys, os, json
from pypdf import PdfReader, PdfWriter

def main():
    if len(sys.argv) != 3:
        print("Usage: fill_art122.py <template> <output>", file=sys.stderr)
        sys.exit(1)

    template_path = sys.argv[1]
    output_path   = sys.argv[2]

    raw = os.environ.get("ART122_FIELDS", "")
    if not raw:
        print("ART122_FIELDS env var is empty", file=sys.stderr)
        sys.exit(1)

    fields = json.loads(raw)

    reader = PdfReader(template_path)
    writer = PdfWriter()
    writer.append(reader)
    writer.update_page_form_field_values(writer.pages[0], fields)

    with open(output_path, "wb") as f:
        writer.write(f)

if __name__ == "__main__":
    main()