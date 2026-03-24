# SequeMusic

Aplicação web desenvolvida no âmbito da unidade curricular de Desenvolvimento Web. A plataforma permite explorar conteúdos musicais, incluindo artistas, músicas, notícias e rankings.

## Descrição

O SequeMusic é uma aplicação que apresenta:

- Notícias recentes sobre cultura musical
- Top músicas mais ouvidas
- Pesquisa por artistas e músicas
- Informação detalhada sobre artistas e músicas

A aplicação permite ainda interação por parte dos utilizadores, como comentários, avaliações e promoção de música.

## Funcionalidades

- Adicionar ratings (avaliações)
- Comentários em músicas
- Visualizar número de streams por plataforma
- Aceder a links das músicas
- Consultar informação de artistas
- Ver notícias relacionadas com música
- Promover músicas (utilizadores premium)

## Tipos de Utilizador

- **Administrador**
  - Gestão completa da aplicação
  - Criar, editar e remover conteúdos

- **Utilizador Registado**
  - Comentar e avaliar músicas
  - Consultar artistas e notícias

- **Utilizador Premium**
  - Promover músicas próprias

## Tecnologias Utilizadas

- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Identity
- Bootstrap
- jQuery
- Chart.js

## Estrutura da Aplicação

A aplicação está organizada em várias secções:

- Página inicial com notícias e top músicas
- Página de artistas
- Página de músicas
- Página de notícias
- Página de géneros
- Área de perfil do utilizador
- Secção de promoção de música

## API

A aplicação disponibiliza endpoints REST para:

### Músicas
- GET `/api/v1/API/Musicas`
- GET `/api/v1/API/Musica/{id}`
- POST `/api/v1/API/CriarMusica`
- PUT `/api/v1/API/AtualizarMusica/{id}`
- DELETE `/api/v1/API/ApagarMusica/{id}`

### Artistas
- GET `/api/v1/API/Artistas`
- GET `/api/v1/API/Artista/{id}`
- POST `/api/v1/API/CriarArtista`
- PUT `/api/v1/API/AtualizarArtista/{id}`
- DELETE `/api/v1/API/ApagarArtista/{id}`

### Notícias
- GET `/api/v1/API/Noticias`
- POST `/api/v1/API/CriarNoticia`
- PUT `/api/v1/API/AtualizarNoticia/{id}`
- DELETE `/api/v1/API/ApagarNoticia/{id}`

### Géneros
- GET `/api/v1/API/Generos`
- GET `/api/v1/API/Genero/{id}`
- POST `/api/v1/API/CriarGenero`
- PUT `/api/v1/API/AtualizarGenero/{id}`
- DELETE `/api/v1/API/ApagarGenero/{id}`

### Outros
- GET `/api/v1/API/StreamingsPorMusica/{musicaId}`
- GET `/api/v1/API/AvaliacoesPorMusica/{musicaId}`

## Deploy

Aplicação disponível em:

https://sequemusic-b9h2embaewajhwgb.uksouth-01.azurewebsites.net

## Credenciais de Teste

- **Administrador**
  - Email: admin@sequemusic.com
  - Password: Admin123!

- **Utilizador**
  - Email: user@sequemusic.com
  - Password: User123!

## Funcionalidades do Sistema

### Utilizador normal
- Ver top músicas
- Pesquisar artistas e músicas
- Comentar e avaliar

### Administrador
- Criar e editar músicas
- Gerir artistas e notícias
- Gerir dados de streaming
- Controlar rankings

## Base de Dados

A aplicação suporta relações:

- 1-N (Artistas → Músicas)
- M-N (Músicas ↔ Artistas)

Operações disponíveis:
- Create
- Read
- Update
- Delete

## Melhorias Futuras

- Upload de imagem de perfil
- Sistema de álbuns
- Melhorias na UX/UI

## Autores

- João Silva (24863)
- Guilherme Aires (23878)
