#!/bin/bash
# ============================================================
# 抖音小游戏 Unity 项目 - 一键推送到 GitHub
# 用法:
#   1. 设置环境变量 GITHUB_TOKEN (需要 repo 权限)
#      export GITHUB_TOKEN="ghp_xxxxxxxxxxxxxxxxxxxx"
#   2. 运行此脚本
#      bash push_to_github.sh [仓库名] [仓库描述]
# ============================================================

set -e

REPO_NAME="${1:-douyin-mini-game-unity}"
REPO_DESC="${2:-抖音小游戏Unity框架 - 接金币游戏 (SingletonMono + EventBus + SDK桥接 + 零预制体)}"
REPO_PRIVATE="${3:-public}"  # public 或 private

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# 检查 Token
if [ -z "$GITHUB_TOKEN" ]; then
    echo "错误: 请先设置 GITHUB_TOKEN 环境变量"
    echo "  export GITHUB_TOKEN=\"ghp_xxxxxxxxxxxxxxxxxxxx\""
    echo ""
    echo "获取 Token: https://github.com/settings/tokens/new?scopes=repo"
    exit 1
fi

# 认证 gh CLI
echo ">>> 认证 GitHub..."
echo "$GITHUB_TOKEN" | gh auth login --with-token 2>/dev/null || {
    # gh 2.4 不支持 --with-token 的旧格式，尝试用 GH_TOKEN 环境变量
    export GH_TOKEN="$GITHUB_TOKEN"
}

# 获取用户名
GH_USER=$(gh api user --jq '.login' 2>/dev/null || echo "")
if [ -z "$GH_USER" ]; then
    echo "认证失败，请检查 Token 是否有效"
    exit 1
fi
echo ">>> 认证成功! 用户: $GH_USER"

# 检查仓库是否已存在
echo ">>> 检查仓库 $REPO_NAME 是否存在..."
if gh repo view "$GH_USER/$REPO_NAME" >/dev/null 2>&1; then
    echo ">>> 仓库已存在，将直接推送"
else
    echo ">>> 创建新仓库: $GH_USER/$REPO_NAME ($REPO_PRIVATE)"
    if [ "$REPO_PRIVATE" = "private" ]; then
        gh repo create "$REPO_NAME" --private --description "$REPO_DESC"
    else
        gh repo create "$REPO_NAME" --public --description "$REPO_DESC"
    fi
    echo ">>> 仓库创建成功!"
fi

# 添加远程仓库
REMOTE_URL="https://$GH_USER:$GITHUB_TOKEN@github.com/$GH_USER/$REPO_NAME.git"

# 检查是否已有 origin 远程
if git remote get-url origin >/dev/null 2>&1; then
    git remote set-url origin "$REMOTE_URL"
else
    git remote add origin "$REMOTE_URL"
fi
echo ">>> 远程仓库已配置"

# 推送
echo ">>> 推送到远程仓库..."
git push -u origin main

# 清理远程 URL 中的 Token (安全)
git remote set-url origin "https://github.com/$GH_USER/$REPO_NAME.git"

echo ""
echo "=========================================="
echo "推送成功!"
echo "仓库地址: https://github.com/$GH_USER/$REPO_NAME"
echo "=========================================="
