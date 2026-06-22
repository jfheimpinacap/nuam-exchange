# 00 — Contexto y decisiones

## Objetivo

Nuam Exchange — Sistema de Gestión Tributaria tiene como objetivo preparar una plataforma para gestionar procesos tributarios de una corredora de bolsa.

## Stack definido

- Frontend: React + TypeScript + Vite.
- Backend: ASP.NET Core Web API .NET 8.
- Base de datos futura: SQL Server 2022.
- ORM futuro: Entity Framework Core con proveedor SQL Server.
- Hosting futuro: Wirenet/Plesk.

## URL pública futura

La URL pública futura será `https://nuam-exchange.jem-nexus.cl` y la API futura quedará bajo `https://nuam-exchange.jem-nexus.cl/api`.

## Decisión de hosting unificado

React compilado será servido por la misma aplicación ASP.NET Core que expone la API. Esto simplifica el despliegue futuro en Plesk, evita configurar múltiples aplicaciones públicas y mantiene una sola superficie HTTP para frontend y backend.

## Estado de despliegue

No existe despliegue todavía. No se tocó Plesk, Wirenet, subdominios, SQL Server remoto ni producción.
