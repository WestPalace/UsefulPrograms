import os
from collections import defaultdict
from itertools import combinations

def read_code_lines(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    # 空行やコメント行を除外（必要に応じて調整）
    return [line.strip() for line in lines if line.strip() and not line.strip().startswith("//")]

def count_duplicate_lines(filepaths):
    line_occurrences = defaultdict(set)  # 行 -> 出現したファイルの集合

    for filepath in filepaths:
        lines = set(read_code_lines(filepath))
        for line in lines:
            line_occurrences[line].add(filepath)

    # 2つ以上のファイルに登場する行だけを抽出
    duplicates = {line: files for line, files in line_occurrences.items() if len(files) > 1}

    print(f"重複した行数: {len(duplicates)}")
    return duplicates

# 🔽 使用例（カレントディレクトリ内の .cs ファイルを対象）
def main():
    folder = "./TargetPrograms"
    cs_files = [os.path.join(folder, f) for f in os.listdir(folder) if f.endswith(".cs")]
    duplicates = count_duplicate_lines(cs_files)

    # 詳細表示（任意）
    # for line, files in duplicates.items():
    #     print(f"{line}  ->  {len(files)}ファイルに登場")

if __name__ == "__main__":
    main()
