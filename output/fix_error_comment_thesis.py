from pathlib import Path
import re
from tempfile import NamedTemporaryFile
from zipfile import ZIP_DEFLATED, ZipFile

from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.oxml.ns import qn
from docx.shared import Pt
from lxml import etree


SOURCE = Path(r"E:\Project_Metroidvania\Metroidvania\output\error_comment_thesis_working.docx")
OUT = Path(r"E:\Project_Metroidvania\Metroidvania\output\error_comment_thesis_fixed.docx")

CJK_RE = re.compile(r"[\u3400-\u9fff]")
ASCII_RE = re.compile(r"[A-Za-z0-9]")


def set_run_font_slots(run, *, ascii_font=None, east_asia_font=None):
    rpr = run._element.get_or_add_rPr()
    rfonts = rpr.rFonts
    if rfonts is None:
        rfonts = rpr._add_rFonts()

    if ascii_font:
        rfonts.set(qn("w:ascii"), ascii_font)
        rfonts.set(qn("w:hAnsi"), ascii_font)

    if east_asia_font:
        rfonts.set(qn("w:eastAsia"), east_asia_font)


def remove_numbering(paragraph):
    ppr = paragraph._element.pPr
    if ppr is None:
        return False

    numpr = ppr.numPr
    if numpr is None:
        return False

    ppr.remove(numpr)
    return True


def is_reference_text(text):
    return bool(re.match(r"^\[\d+\]", text.strip()))


def strip_red_template_callouts(docx_path):
    ns = {
        "w": "http://schemas.openxmlformats.org/wordprocessingml/2006/main",
        "v": "urn:schemas-microsoft-com:vml",
        "wps": "http://schemas.microsoft.com/office/word/2010/wordprocessingShape",
        "a": "http://schemas.openxmlformats.org/drawingml/2006/main",
    }

    with ZipFile(docx_path) as zin:
        document = etree.fromstring(zin.read("word/document.xml"))
        removed = 0

        for run in list(document.xpath(".//w:r", namespaces=ns)):
            has_red_vml = bool(run.xpath(".//v:shape[@strokecolor='red']", namespaces=ns))
            has_red_drawing = bool(
                run.xpath(".//wps:wsp[.//a:ln//a:srgbClr[@val='FF0000']]", namespaces=ns)
            )
            if has_red_vml or has_red_drawing:
                parent = run.getparent()
                if parent is not None:
                    parent.remove(run)
                    removed += 1

        document_xml = etree.tostring(
            document,
            encoding="UTF-8",
            xml_declaration=True,
            standalone=True,
        )

        with NamedTemporaryFile(delete=False, suffix=".docx", dir=docx_path.parent) as tmp:
            tmp_path = Path(tmp.name)

        with ZipFile(tmp_path, "w", compression=ZIP_DEFLATED) as zout:
            for item in zin.infolist():
                payload = document_xml if item.filename == "word/document.xml" else zin.read(item.filename)
                zout.writestr(item, payload)

    tmp_path.replace(docx_path)
    return removed


doc = Document(SOURCE)
styles_by_id = {style.style_id: style for style in doc.styles}

# Match the detector's expected title and body geometry exactly.
styles_by_id["af7"].paragraph_format.first_line_indent = Pt(24)
styles_by_id["ac"].paragraph_format.space_after = Pt(1)
styles_by_id["af0"].paragraph_format.space_before = Pt(0.5)
styles_by_id["a3"].paragraph_format.space_before = Pt(0.5)

for sid in ("TOC1", "TOC2", "TOC3"):
    styles_by_id[sid].paragraph_format.alignment = WD_ALIGN_PARAGRAPH.DISTRIBUTE

counts = {
    "toc_paragraphs": 0,
    "toc_english_runs": 0,
    "chapter_headings": 0,
    "section_headings": 0,
    "subsection_headings": 0,
    "body_numbering_removed": 0,
    "normal_body_left_indent_fixed": 0,
    "references_left_aligned": 0,
    "zh_abstract_heading": 0,
    "zh_abstract_body": 0,
    "zh_keyword": 0,
    "en_abstract_body": 0,
    "en_keyword": 0,
}

for index, paragraph in enumerate(doc.paragraphs):
    text = paragraph.text.strip()
    sid = paragraph.style.style_id
    fmt = paragraph.paragraph_format

    if sid in {"TOC1", "TOC2", "TOC3"}:
        fmt.alignment = WD_ALIGN_PARAGRAPH.DISTRIBUTE
        counts["toc_paragraphs"] += 1
        if sid == "TOC3":
            for run in paragraph.runs:
                if ASCII_RE.search(run.text):
                    set_run_font_slots(run, ascii_font="Times New Roman")
                    counts["toc_english_runs"] += 1

    if sid == "ac":
        fmt.space_after = Pt(1)
        counts["chapter_headings"] += 1

    if sid == "af0":
        fmt.space_before = Pt(0.5)
        fmt.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        counts["section_headings"] += 1

    if sid == "a3":
        fmt.space_before = Pt(0.5)
        fmt.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
        counts["subsection_headings"] += 1

    if sid == "af7":
        if remove_numbering(paragraph):
            fmt.left_indent = None
            fmt.first_line_indent = None
            counts["body_numbering_removed"] += 1

    # Three paragraphs in the summary section use a direct 21 pt left indent.
    if sid == "a" and fmt.left_indent is not None and round(fmt.left_indent.pt, 1) == 21.0:
        fmt.left_indent = Pt(24)
        counts["normal_body_left_indent_fixed"] += 1

    if is_reference_text(text):
        fmt.alignment = WD_ALIGN_PARAGRAPH.LEFT
        counts["references_left_aligned"] += 1

    if sid == "af5" and text.replace(" ", "") == "摘要":
        fmt.space_before = Pt(2)
        fmt.line_spacing = 1.5
        counts["zh_abstract_heading"] += 1

    if index in (38, 39, 40):
        fmt.line_spacing = 1.25
        for run in paragraph.runs:
            if CJK_RE.search(run.text):
                set_run_font_slots(run, east_asia_font="宋体")
        counts["zh_abstract_body"] += 1

    if text.startswith("关键词："):
        fmt.first_line_indent = Pt(0)
        for run in paragraph.runs:
            if CJK_RE.search(run.text):
                set_run_font_slots(run, east_asia_font="黑体")
        counts["zh_keyword"] += 1

    if index in (44, 45, 46):
        fmt.line_spacing = 1.25
        counts["en_abstract_body"] += 1

    if text.startswith("Keywords:"):
        fmt.line_spacing = 1.0
        counts["en_keyword"] += 1

doc.save(OUT)
counts["red_template_callouts_removed"] = strip_red_template_callouts(OUT)
print(f"wrote={OUT}")
for key, value in counts.items():
    print(f"{key}={value}")
