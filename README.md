# 每日多言

~~谁会不想在 ClassIsland 上摆上好多个一言组件看个爽呢~~

**因为本人并不是很会 Avalonia 开发加上时间有限 (神秘高三牲), 所以项目几乎完全由 DS V4 Flash 编写, 相关代码参考 [ExtraIsland](https://docs.lipoly.ink/ExtraIsland/), 我仅做测试 (在我们班白板机上, 工作良好😋)**

## 内容来源

| 来源 | ID | 默认启用 | 默认权重 | 描述 |
|------|----|----------|----------|------|
| **每日一句** | `dlystc` | ✅ 是 | 2 | 来自 [dlystc API](https://dlystc.unknownmp.top/) |
| **一言** | `hitokoto` | ❌ 否 | 1 | 来自 [v1.hitokoto.cn](https://v1.hitokoto.cn/) |
| **今日诗词** | `jinrishici` | ❌ 否 | 1 | 来自 [v1.jinrishici.com](https://v1.jinrishici.com/) |
| **诏预** | `saintic` | ❌ 否 | 1 | 来自 [Saintic 句子 API](https://hub.saintic.com/) |

*关于 DlyStc 源: 这是我的一个[项目](https://github.com/dlystc)(提供经过过滤的 Hitokoto 数据集), 所以私心提高了优先级*

## 构建

```bash
dotnet publish -c Release '-p:CreateCipx=true'
```