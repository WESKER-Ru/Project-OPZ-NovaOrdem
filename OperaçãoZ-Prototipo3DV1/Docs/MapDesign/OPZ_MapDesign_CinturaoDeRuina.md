# Operação Z / A Nova Ordem — Design do Mapa Principal
## "Cinturão de Ruína" (Codinome interno: MAP_01)

---

## 1. VISÃO MACRO DO MAPA

### Conceito Geral
O mapa "Cinturão de Ruína" é um campo de batalha pós-colapso de escala militar real, organizado em torno de uma cidade central densa que funciona como o coração econômico, tático e narrativo da partida. Quatro biomas táticos distintos irradiam a partir desse núcleo urbano, cada um impondo um ritmo de combate diferente e forçando o jogador a adaptar sua composição de força conforme avança.

### Identidade Tática
O mapa não é simétrico no sentido geométrico puro — é **simetricamente justo**. Os dois spawns do MVP (AR no noroeste/deserto, EG no sudeste/pântano) têm acesso equivalente ao centro, mas por rotas com personalidades táticas opostas. Isso reforça a assimetria de facções sem criar injustiça espacial.

### Fantasia Militarizada
O jogador deve sentir que está operando em um teatro de operações real:
- O deserto evoca avanço blindado em terreno exposto
- O pântano evoca infiltração difícil e desgaste
- A planície evoca manobra de campo aberto e pressão frontal
- A floresta evoca emboscada e progressão lenta por corredores
- A cidade evoca combate urbano brutal, bloco a bloco

### Por que combina com OPZ
O mundo de OPZ é um mundo em ruína onde facções disputam os restos da civilização. A cidade central é literalmente o prêmio — infraestrutura, recursos, posição. Os biomas ao redor são o preço: terreno hostil, infectados, exposição. O mapa transforma o lore em gameplay.

---

## 2. ESTRUTURA GEOMÉTRICA

### Forma Geral
Losango suave (diamante rotacionado ~45°) com cantos arredondados. Dimensão aproximada: **300×300 unidades Unity** (área jogável efetiva ~280×280). Bordas naturais: penhascos, escombros intransponíveis, água profunda.

### Layout dos 4 Polos

```
Orientação do losango (rotação 45°):

         NORTE
           ◇
     NW  /   \  NE
       /       \
  W  ◇  CIDADE  ◇  E
       \       /
     SW  \   /  SE
           ◇
          SUL
```

| Polo | Posição | Bioma | Spawn MVP |
|------|---------|-------|-----------|
| Noroeste | Canto NW | **Deserto** | ✅ Spawn AR |
| Nordeste | Canto NE | **Planície Aberta** | — (futuro Spawn C) |
| Sudoeste | Canto SW | **Floresta Fechada** | — (futuro Spawn D) |
| Sudeste | Canto SE | **Pântano** | ✅ Spawn EG |

### Cidade Central — "Nova Cinza"
- Posição: centro geométrico exato do mapa
- Raio aproximado: 40-50 unidades
- Formato: malha urbana retangular irregular, levemente rotacionada em relação ao mapa
- Densidade: alta, com blocos de 3-5 edifícios agrupados, ruas de 4-6u de largura
- Não é um quadrado perfeito — tem bordas irregulares onde a cidade "se desfaz" nos biomas

### Hidrografia — Rio Seco
- Um rio atravessa o mapa de **nordeste a sudoeste**
- No nordeste (planície): rio largo, raso, vadeável em pontos marcados
- No centro: passa pela borda sul da cidade com 2 pontes
- No sudoeste (floresta): rio mais estreito, margens densas, 1 ponte + 1 passagem por troncos
- No trecho do pântano (SE): o rio se dissolve em alagados e perde definição

### Estradas Principais
- **Rota Norte**: estrada de asfalto quebrado conectando Spawn AR (NW) à entrada norte da cidade
- **Rota Sul**: trilha larga conectando Spawn EG (SE) à entrada sul da cidade
- **Anel Externo**: caminho que contorna a cidade por fora, passando pelos 4 biomas (mais lento, mas evita o centro)
- **Travessas**: trilhas menores cortando dentro de cada bioma

### Raid Island — "Ilha do Reservatório"
- Posição: **leste do mapa**, em um reservatório de água (lago artificial)
- Acesso: uma única ponte/causeway estreita pelo lado da planície
- Tamanho: ~25×20u — espaço para 2-3 nós de recurso de alto valor
- Justificativa: funciona como objetivo de risco/recompensa lateral. Não é obrigatória para vencer, mas quem a controla ganha vantagem econômica. Posição leste significa que ambos os spawns têm distância similar

---

## 3. LEITURA TÁTICA DOS BIOMAS

### 3.1 DESERTO (Noroeste)

| Aspecto | Detalhe |
|---------|---------|
| **Função estratégica** | Polo de avanço rápido e exposição. Quem controla o deserto tem mobilidade, mas sem cobertura. |
| **Mobilidade** | Alta. Terreno plano a ondulado, poucos obstáculos. NavMesh limpo. |
| **Visão** | Longa. Linha de visão de 20-30u sem obstrução. Dunas baixas quebram parcialmente. |
| **Cobertura** | Mínima. Carcaças de veículos, crateras e escombros isolados. |
| **Risco** | Alto para infantaria desprotegida. Bom para veículos. |
| **Valor tático** | Rota mais rápida para a entrada norte da cidade. Recursos iniciais do Spawn AR. |
| **Combate favorecido** | Manobra aberta, avanço blindado, combate de alcance. |
| **Infectados** | Baixa presença. Vagantes isolados perto de carcaças. |
| **Terreno Unity** | Sand material, dunas via heightmap suave, props esparsos (veículos, ossos, postes). |

### 3.2 PÂNTANO (Sudeste)

| Aspecto | Detalhe |
|---------|---------|
| **Função estratégica** | Polo de desgaste e infiltração difícil. Terreno penaliza pressa e recompensa paciência. |
| **Mobilidade** | Baixa. Áreas alagadas reduzem velocidade (NavMesh area cost alto). Caminhos secos são previsíveis. |
| **Visão** | Média-curta. Vegetação rasteira densa, neblina baixa, árvores mortas. |
| **Cobertura** | Boa cobertura irregular. Poças, arbustos, troncos caídos. |
| **Risco** | Alto para movimentação em massa. Rotas estreitas → emboscada. |
| **Valor tático** | Rota alternativa para flanco sul da cidade. Recursos iniciais do Spawn EG. |
| **Combate favorecido** | Desgaste, emboscada, infantaria leve, combate de curto alcance. |
| **Infectados** | Alta presença. Vagantes em grupos nos alagados. Corredores perto das trilhas secas. |
| **Terreno Unity** | Mud/swamp material, água rasa (plane com shader), fog volume localizado, vegetação GPU instanced. |

### 3.3 PLANÍCIE ABERTA (Nordeste)

| Aspecto | Detalhe |
|---------|---------|
| **Função estratégica** | Polo de manobra em escala. Campo de batalha clássico para confrontos grandes. |
| **Mobilidade** | Muito alta. Terreno plano com grama. Ideal para formações. |
| **Visão** | Muito longa. Sem obstrução significativa. |
| **Cobertura** | Quase nenhuma. Pequenas elevações, muros de fazenda arruinados. |
| **Risco** | Extremo para tropas isoladas. Recompensa composição de força equilibrada. |
| **Valor tático** | Acesso à Raid Island. Rota de flanco NE. Recursos intermediários espalhados. |
| **Combate favorecido** | Combate em escala, formações, avanço coordenado, campo de tiro longo. |
| **Infectados** | Baixa. Corredores isolados patrulhando. |
| **Terreno Unity** | Grass terrain, props de fazenda destruída, muros baixos, feno. Performance excelente. |

### 3.4 FLORESTA FECHADA (Sudoeste)

| Aspecto | Detalhe |
|---------|---------|
| **Função estratégica** | Polo de emboscada e infiltração. Quem controla os corredores florestais controla o flanco SW. |
| **Mobilidade** | Baixa-média. Árvores bloqueiam movimento livre. NavMesh carving essencial. |
| **Visão** | Curta. Dossel denso, troncos grossos, arbustos. Visão de 5-8u. |
| **Cobertura** | Excelente. Árvores, rochas, ruínas florestais. |
| **Risco** | Alto para quem não reconhece. Baixo para quem embosca. |
| **Valor tático** | Rota de flanco SW para a cidade. Recursos intermediários valiosos entre as árvores. |
| **Combate favorecido** | Emboscada, hit-and-run, infantaria em corredores, combate de curto alcance. |
| **Infectados** | Média-alta. Vagantes entre as árvores. Corredores surgem quando tropas entram. |
| **Terreno Unity** | Dense tree placement via terrain trees (GPU instancing), undergrowth como detail meshes, rocks como prefabs. LOD agressivo essencial. |

---

## 4. FLUXO DE MOVIMENTAÇÃO

### 4.1 Rotas do Spawn AR (NW/Deserto) → Centro

| Rota | Tipo | Caminho | Risco | Velocidade |
|------|------|---------|-------|------------|
| **R1 — Frontal Norte** | Principal | Estrada do deserto → entrada norte da cidade | Médio (previsível) | Rápida |
| **R2 — Flanco Leste** | Lateral | Deserto → borda da planície → leste da cidade | Alto (exposição total) | Rápida |
| **R3 — Flanco Oeste** | Infiltração | Deserto → borda da floresta → oeste da cidade | Médio (emboscada) | Lenta |
| **R4 — Anel Externo NE** | Contorno | Deserto → planície → Raid Island / leste | Baixo (longe do centro) | Média |

### 4.2 Rotas do Spawn EG (SE/Pântano) → Centro

| Rota | Tipo | Caminho | Risco | Velocidade |
|------|------|---------|-------|------------|
| **R5 — Frontal Sul** | Principal | Trilha do pântano → travessia do rio → entrada sul da cidade | Médio-alto (rio + pântano) | Lenta-média |
| **R6 — Flanco Leste** | Lateral | Pântano → borda da planície → ponte leste → cidade | Médio (exposição na planície) | Média |
| **R7 — Flanco Oeste** | Infiltração | Pântano → borda da floresta → rio estreito → oeste da cidade | Alto (floresta + infectados) | Lenta |
| **R8 — Anel Externo SW** | Contorno | Pântano → floresta → SW da cidade | Baixo (longe do centro) | Lenta |

### 4.3 Como a Cidade Conecta/Interrompe

A cidade central funciona como **rótula tática**:
- Quem entra pela norte sai pela sul (e vice-versa) — mas precisa cruzar ruas contestadas
- A via principal N-S é a rota mais óbvia e mais perigosa
- Vias laterais L-O permitem contornar por dentro, mas com visão quebrada
- A cidade **não bloqueia** o fluxo — ela o **canaliza e retarda**, forçando decisões

### 4.4 Transições entre Biomas

As transições são graduais (20-30u de zona de transição):
- **Deserto → Planície**: areia dá lugar a terra seca e grama rala. Ruínas de fazenda marcam a fronteira.
- **Deserto → Floresta**: areia com arbustos → árvores esparsas → dossel fechado. ~25u de gradiente.
- **Pântano → Planície**: alagado diminui, grama emerge, terreno sobe levemente.
- **Pântano → Floresta**: vegetação aquática → vegetação densa terrestre. Mais infectados nesta faixa.
- **Qualquer bioma → Cidade**: ruínas suburbanas (casas esparsas, muros, cercas) → cidade densa. Buffer de 15-20u.

---

## 5. ZONAS DE CONFLITO

### Hotspots Principais (Tier 1 — disputa constante)

| Zona | Localização | Por quê |
|------|-------------|---------|
| **Praça Central** | Centro da cidade | Cruzamento das 2 vias principais. Recursos de alto valor. Quem segura a praça controla o fluxo urbano. |
| **Ponte Sul** | Borda sul da cidade sobre o rio | Único cruzamento direto do rio perto do centro. Choke natural. |
| **Ponte Leste** | Borda leste da cidade | Acesso à planície e indiretamente à Raid Island. |

### Hotspots Secundários (Tier 2 — disputa oportunista)

| Zona | Localização | Por quê |
|------|-------------|---------|
| **Raid Island** | Leste, no reservatório | Recursos de alto valor, posição isolada. |
| **Cruzamento Florestal** | Centro-SW, onde rio cruza a floresta | Ponte estreita + cobertura = emboscada ideal. |
| **Ruínas da Fazenda** | Transição deserto-planície (NE) | Recursos intermediários + posição de observação. |
| **Depósito Militar** | Dentro da cidade, quadrante NW urbano | Recurso de metal de alto valor dentro da cidade. |
| **Estação de Tratamento** | Dentro da cidade, quadrante SE urbano | Recurso de fuel de alto valor dentro da cidade. |

### Zonas de Emboscada
- Floresta: qualquer corredor entre árvores densas
- Cidade: becos e vias secundárias perpendiculares à rota N-S
- Pântano: trilhas secas entre alagados (canaliza o inimigo)
- Transição floresta-cidade: subúrbio arruinado com muros e casas

### Zonas de Avanço Rápido
- Estrada do deserto (R1)
- Planície aberta (NE inteiro)
- Via principal N-S dentro da cidade (se não contestada)

### Zonas de Travamento (Stalemate Risk)
- Praça Central (se ambos empurram ao mesmo tempo)
- Ponte Sul (se defendida pesadamente)
- Mitigação: sempre há rota lateral para quebrar o impasse

---

## 6. CHOKE POINTS E CONTROLE DE TERRENO

### Choke Points Mapeados

| Choke | Localização | Tipo | Bypass |
|-------|-------------|------|--------|
| **Ponte Sul** | Rio, borda sul da cidade | Ponte sobre rio (8u largura) | Vadear o rio 40u a leste (lento, exposto) |
| **Ponte Leste** | Rio, borda leste da cidade | Ponte sobre rio (10u largura) | Contornar pela planície (longe) |
| **Ponte Florestal** | Rio, centro da floresta | Ponte estreita (5u) + troncos | Vadear 30u ao norte (muito lento) |
| **Garganta do Deserto** | Entre dunas altas, NW | Passagem entre elevações (12u) | Contornar pelas dunas (expõe flanco) |
| **Via Principal N-S** | Dentro da cidade | Rua de 6u entre edifícios | Vias secundárias L-O (mais lentas, visão quebrada) |
| **Causeway da Raid Island** | Ponte para a ilha | Ponte única (6u) | Nenhum (esse é o ponto) |

### Princípios Anti-Travamento
1. **Todo choke tem bypass** (exceto Raid Island, que é objetivo de risco por design)
2. Nenhum choke é obrigatório para atingir o centro — sempre há rota alternativa
3. Chokes recompensam defesa posicional, mas não impedem manobra
4. O jogador escolhe: empurrar o choke (custo alto, ganho rápido) ou contornar (custo baixo, ganho lento)

---

## 7. SPAWNS MINIMALISTAS

### Design do Spawn

Ambos os spawns seguem o mesmo template:

| Elemento | Especificação |
|----------|---------------|
| **Área livre** | Plataforma plana de ~30×30u, limpa de obstáculos |
| **Tenda inicial** | 1 estrutura pré-posicionada (HQ Tier 1) no centro da área |
| **Buffer de construção** | Raio de ~20u ao redor da tenda sem obstruções de terreno |
| **Recursos iniciais seguros** | 2 nós de Supplies + 1 nó de Metal a 15-25u do HQ |
| **Saída primária** | 1 estrada/trilha larga (10u) direcionada ao centro |
| **Saída secundária** | 1 trilha lateral (6u) direcionada ao bioma adjacente |
| **Proteção natural** | Bordas do spawn parcialmente protegidas por terreno (penhascos/alagados/árvores), impedindo rush de 360° |
| **Infectados** | Zero dentro do spawn. Primeiros aparecem a 40u+ da tenda. |

### Spawn AR (Deserto NW)
- Platô elevado com rampa de saída para sudeste (rota principal)
- Lado norte e oeste: penhascos intransponíveis (borda do mapa)
- Saída lateral: trilha para leste (planície) entre dunas
- Visual: areia compacta, carcaças de veículos decorativas na borda, horizonte aberto ao sul

### Spawn EG (Pântano SE)
- Ilha seca elevada em meio ao alagado
- Lado sul e leste: pântano profundo intransponível (borda do mapa)
- Saída principal: trilha seca para noroeste (rio e cidade)
- Saída lateral: trilha para oeste (floresta) sobre terra firme
- Visual: lama seca compacta, árvores mortas decorativas, neblina na periferia

### Justiça Espacial
- Distância de ambos os spawns ao centro da cidade: ~120u (± 5u de tolerância)
- Ambos têm 3 nós de recurso seguros dentro do buffer
- Ambos têm 2 saídas funcionais
- Ambos têm proteção natural contra rush direto nos primeiros 40u
- A diferença está no **bioma**, não na **vantagem posicional**

---

## 8. RECURSOS E EXPANSÃO

### Camada 1 — Recursos Iniciais (Seguros)

| Recurso | Quantidade | Distância do HQ | Risco |
|---------|-----------|------------------|-------|
| Supplies (madeira/escombro) | 2 nós × 500 cada | 15-20u | Nenhum |
| Metal (sucata) | 1 nó × 400 | 20-25u | Nenhum |

Suficiente para: construir primeiro quartel, treinar primeiros workers e soldados, iniciar expansão.

### Camada 2 — Recursos Intermediários (Expansão com risco leve)

| Recurso | Localização | Distância do HQ | Risco |
|---------|-------------|------------------|-------|
| Supplies × 2 | Zona de transição bioma→cidade (subúrbio) | 50-70u | Infectados vagantes, possível encontro com scout inimigo |
| Metal × 1 | Centro do bioma próprio (deserto ou pântano) | 40-60u | Infectados leves, exposição |
| Fuel × 1 | Borda do bioma adjacente (planície ou floresta) | 60-80u | Risco médio — fora do "território seguro" |

### Camada 3 — Recursos Contestados (Alto valor, alto risco)

| Recurso | Localização | Risco |
|---------|-------------|-------|
| Metal premium × 1 | Depósito Militar (dentro da cidade, NW) | Combate urbano, posição contestada |
| Fuel premium × 1 | Estação de Tratamento (dentro da cidade, SE) | Combate urbano, posição contestada |
| Supplies premium × 2 | Praça Central | Hotspot principal, máximo risco |
| Misto alto valor × 2-3 | Raid Island | Ponte única, posição isolada |

### Lógica de Risco/Recompensa

```
SEGURO ──────────────────────── CONTESTADO
HQ spawn    subúrbio    borda bioma    cidade    praça central    raid island
(baixo)     (leve)      (médio)        (alto)    (máximo)         (máximo+isolado)
```

### Balanceamento Posicional
- Metal premium da cidade está no quadrante NW (mais perto de AR) → compensado pelo Fuel premium estar no quadrante SE (mais perto de EG)
- Raid Island está equidistante de ambos (~90u)
- Recursos da praça central são equidistantes por design

---

## 9. LÓGICA PvPvE

### Distribuição de Infectados por Zona

| Zona | Vagantes | Corredores | Pressão Total |
|------|----------|------------|---------------|
| Spawns (40u buffer) | 0 | 0 | Nenhuma |
| Deserto (aberto) | Poucos, isolados | Raros | Baixa |
| Planície | Raros | Poucos, patrulha | Baixa |
| Floresta | Médios, em grupos de 2-3 | Médios | Média-Alta |
| Pântano | Muitos, em grupos | Médios | Alta |
| Subúrbio (transição) | Poucos | Poucos | Média |
| Cidade (periférica) | Médios | Poucos | Média |
| Cidade (centro/praça) | Muitos | Muitos | Alta |
| Raid Island | Poucos mas densos | Vários | Alta (espaço confinado) |

### Como o PvE Afeta o Jogo

**Pressão na floresta e pântano:**
- Dificulta flanquear por esses biomas sem limpar infectados primeiro
- Recompensa recon: saber onde estão os grupos antes de avançar
- Workers precisam de escolta para coletar recursos intermediários

**Pressão na cidade:**
- A praça central é perigosa não só por PvP, mas por concentração de infectados
- Forçar avanço urbano sem limpar = perder unidades para infectados durante o combate PvP
- O jogador que limpa infectados na cidade ganha vantagem posicional

**Pressão na Raid Island:**
- Infectados no espaço confinado da ilha forçam investimento para tomar
- Não basta mandar 2 workers — precisa de combatentes

**Regra de justiça:** a pressão PvE é simétrica em relação aos spawns. O pântano (Spawn EG) tem mais infectados na base, mas o deserto (Spawn AR) tem menos cobertura. Tradeoffs diferentes, pressão total equivalente.

### Spawning de Infectados (Design Rule)
- Infectados NÃO respawnam infinitamente — cada grupo tem spawn fixo
- Limpar uma área garante segurança temporária (5-8 minutos antes de repopular levemente)
- Isso impede que infectados dominem o late game mas mantém pressão no early/mid

---

## 10. IMPLICAÇÕES DE IMPLEMENTAÇÃO EM UNITY

### NavMesh

| Risco | Zona | Solução |
|-------|------|---------|
| Árvores bloqueando pathing | Floresta | NavMesh Obstacle em árvores grossas. Árvores finas = apenas visual, sem collider. Corredores de 4u mínimo entre obstáculos. |
| Água impedindo travessia | Rio, pântano | NavMesh Area "Water" com cost alto (não intransponível). Pontes = NavMesh normal. Pântano raso = area com cost 3-4×. |
| Cidade densa | Centro urbano | Edifícios como NavMesh Obstacles com carving. Ruas garantem 4-6u mínimo. Testar pathfinding com 20+ unidades simultâneas. |
| Unidades ficando presas | Transições de bioma | Garantir que heightmap transitions são suaves (slope < 35°). Bake NavMesh com agent radius adequado. |

### Performance

| Risco | Zona | Solução |
|-------|------|---------|
| Draw calls da floresta | SW | GPU instancing obrigatório para árvores. LOD 3 níveis (50u/100u/150u). Terrain trees, não prefabs individuais. |
| Overdraw da cidade | Centro | Occlusion Culling ativo. Edifícios com LOD 2 níveis. Interiores não renderizados (prédios são volumes sólidos). |
| Vegetação do pântano | SE | Detail meshes com GPU instancing. Distance fade a 80u. Fog volume como shader, não particle system. |
| Mapa grande (300×300) | Global | Terrain de 512×512 ou 1024×1024 heightmap. Splat map com 4-6 layers máximo (URP limit). Se necessário, dividir em 2×2 tiles. |
| Unidades em massa | Qualquer | NavMesh agent com obstacle avoidance quality "Medium" ou "Low" para unidades longe da câmera. |

### Leitura Visual

| Risco | Zona | Solução |
|-------|------|---------|
| Floresta esconde unidades | SW | Dossel parcialmente transparente em câmera alta (shader com alpha baseado em ângulo da câmera). Ou: árvores ficam semi-transparentes quando unidade selecionada está embaixo. |
| Cidade esconde combate | Centro | Telhados dos prédios com transparência ao zoom out. Ou: telhados removidos completamente (visão de planta). |
| Pântano confuso visualmente | SE | Cor dominante distinta (verde-azulado escuro). Diferenciação clara de água rasa vs terra. |
| Minimapa ilegível | Global | Cores de bioma fortemente contrastadas: deserto=bege, pântano=verde escuro, planície=verde claro, floresta=verde oliva, cidade=cinza, rio=azul, spawns=cor da facção. |

### Hierarquia de Densidade Visual
```
Menor densidade ◄──────────────────────► Maior densidade
Deserto < Planície < Pântano < Floresta < Cidade
```
Isso coincide com a curva de performance: zonas de alta densidade visual estão concentradas e podem ser culled quando a câmera está longe.

---

## 11. DIAGRAMA TEXTUAL TOP-DOWN

```
                            N
                            ▲
                ┌───────────────────────┐
               ╱    DESERTO              ╲
              ╱   ┌─────┐                 ╲
             ╱    │SP-AR│  ···dunas···      ╲
            ╱     └──┬──┘                    ╲
           ╱    ·····│·····  Garganta         ╲
          ╱  ~~~~~~~~│~~~~~~~~ ←rio raso       ╲
         │   FLORESTA│         PLANÍCIE         │
         │   🌲🌲🌲  │  ┌──ruínas──┐  ···      │
         │   🌲🌲🌲  │  │ fazenda  │  ···      │
         │   🌲🌲    │  └──────────┘    ···     │
    W ◄  │   🌲 ┌────┴────────────────┐   ···  │  ► E
         │     │   NOVA CINZA         │    ·    │
         │  🌲 │  ══N-S══  Depósito   │ ┌─────┐│
         │  🌲 │  ║Praça║  Militar    │ │RAID │││
         │  🌲 │  ║ CTR ║            ▓│ │ISLE │││ ← lago
         │   🌲│  ══════  Estação    ▓│ └──┬──┘│
         │   🌲│          Tratamento  │    │    │
         │ ~~~~├──ponte S─────────────┘ causeway│
         │~~~~~│          ←rio                  │
          ╲ 🌲 │ponte florest.                 ╱
           ╲🌲🌲│                  ···        ╱
            ╲   │        PÂNTANO  ·~·~·     ╱
             ╲  │  ·~·~·  ┌─────┐ ·~·~·  ╱
              ╲ │  ·~·~·  │SP-EG│ ·~·   ╱
               ╲│         └─────┘      ╱
                └───────────────────────┘
                            ▼
                            S

LEGENDA:
SP-AR = Spawn Aliança Renovada (NW)     ═══ = Via principal
SP-EG = Spawn Estado de Guerra (SE)     ─── = Estrada/trilha
🌲    = Floresta densa                   ~~~ = Rio/água
···   = Planície/campo aberto            ·~· = Pântano/alagado
▓     = Parede de edifícios              │   = Rota de conexão
```

---

## 12. CHECKLIST DE VALIDAÇÃO

| # | Pergunta | Resposta Esperada |
|---|----------|-------------------|
| 1 | O mapa é legível em câmera alta? | ✅ Sim — biomas têm cores e densidades distintas, cidade é bloco visual coeso, rio é linha azul clara. |
| 2 | Os 4 polos são realmente distintos? | ✅ Sim — cada bioma impõe mobilidade, visão e ritmo de combate diferentes. |
| 3 | O centro é realmente valioso? | ✅ Sim — recursos premium, posição de controle de fluxo, hub de rotas. |
| 4 | Os spawns são justos? | ✅ Sim — equidistantes do centro, mesma quantidade de recursos seguros, proteção natural equivalente. |
| 5 | Existe rota de flanco funcional? | ✅ Sim — mínimo 2 por spawn (bioma adjacente + anel externo). |
| 6 | O mapa permite combate militarizado moderno sem perder a lógica RTS? | ✅ Sim — terreno importa, flanco importa, cobertura importa, mas economia e base continuam centrais. |
| 7 | O pathing parece viável? | ✅ Sim — corredores mínimos de 4u, NavMesh areas para custo diferenciado, sem dead-ends. |
| 8 | O minimapa ficaria claro? | ✅ Sim — 5 cores de bioma + azul do rio + cinza da cidade + vermelho/azul dos spawns. |
| 9 | O PvE pressiona sem dominar? | ✅ Sim — infectados concentrados em zonas específicas, spawns protegidos, limpeza temporária funciona. |
| 10 | O mapa funciona no MVP com 2 facções? | ✅ Sim — desenhado para AR vs EG, escalável para 4 facções depois. |
| 11 | Performance é viável em URP? | ✅ Sim — densidade visual concentrada e cullable, terrain padrão, GPU instancing, LOD agressivo. |
| 12 | Raid Island justifica existir? | ✅ Sim — objetivo lateral de risco/recompensa, equidistante, não obrigatória. |
| 13 | Existe risco de stalemate? | ⚠️ Baixo — todo choke tem bypass, cidade tem múltiplas entradas, anel externo permite contorno total. |
| 14 | Workers conseguem coletar sem morrer? | ✅ Sim — recursos iniciais no buffer seguro, intermediários com escolta leve, contestados exigem controle de área. |
