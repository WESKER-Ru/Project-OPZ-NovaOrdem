# GitHub Automation — OPZ / A Nova Ordem

## Arquivos de automação

| Arquivo | Função |
|---|---|
| `.github/workflows/unity-ci.yml` | CI que roda em todo push/PR para `main`, `fix/**`, `feature/**`, `claude/**` |
| `.github/pull_request_template.md` | Template automático preenchido ao abrir um PR |
| `Docs/DevOps/GITHUB_AUTOMATION_SETUP.md` | Este documento |

---

## O que o CI valida

1. **ProjectVersion.txt existe** — confirma que o projeto Unity está no repo
2. **9 scripts críticos existem** — CommandSystem, GameManager, SelectionManager, RTSCameraController, UnitBase, CombatUnit, WorkerUnit, ProductionQueue, EconomyManager
3. **Sem conflito de tecla S** — verifica que `sKey.isPressed` não voltou para RTSCameraController
4. **Namespaces OPZ** — aviso (não falha) se script fora de `Proto/` não declara `namespace OPZ.*`

> O CI **não** compila C# nem roda Unity em batch mode — requer licença Unity serial.
> Para habilitar compilação real, adicione `game-ci/unity-builder@v4` com secrets `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`.

---

## Fluxo de trabalho recomendado

```
main (protegido)
  └── fix/<descricao>     ← bug fixes
  └── feature/<descricao> ← novas features
  └── claude/<descricao>  ← sessões Claude Code
```

1. Crie branch a partir de `main` atualizado
2. Faça commits com mensagens descritivas
3. Push → abra PR → CI roda automaticamente
4. Reviewer usa checklist do PR template
5. Merge squash ou merge commit (não rebase forçado)

---

## Proteção de branch (configurar no GitHub)

Em **Settings → Branches → Add rule** para `main`:
- [x] Require a pull request before merging
- [x] Require status checks: `validate-structure`
- [x] Do not allow bypassing the above settings

---

## Secrets necessários para CI completo (futuro)

| Secret | Descrição |
|---|---|
| `UNITY_LICENSE` | Conteúdo do arquivo `.ulf` de licença Unity |
| `UNITY_EMAIL` | Email da conta Unity |
| `UNITY_PASSWORD` | Senha da conta Unity |

Referência: https://game.ci/docs/github/activation
