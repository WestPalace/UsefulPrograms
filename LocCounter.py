import os

def count_loc(filepath):
    total_lines = 0
    code_lines = 0

    with open(filepath, 'r', encoding='utf-8') as f:
        for line in f:
            total_lines += 1
            stripped = line.strip()

            # 空行、コメント（単一行）を除外
            if stripped and not stripped.startswith("//"):
                code_lines += 1

    return total_lines, code_lines

# 🔽 使用例：ディレクトリ内のC#ファイルすべてを対象に集計
def main():
    folder = "./TargetPrograms"
    cs_files = [os.path.join(folder, f) for f in os.listdir(folder) if f.endswith(".cs")]

    total_loc = 0
    total_code_loc = 0

    for filepath in cs_files:
        total, code = count_loc(filepath)
        print(f"{os.path.basename(filepath)}: 総行数={total}, コード行数={code}")
        total_loc += total
        total_code_loc += code

    print("\n--- 合計 ---")
    print(f"総行数: {total_loc}")
    print(f"コード行数（コメント・空行除外）: {total_code_loc}")

if __name__ == "__main__":
    main()
