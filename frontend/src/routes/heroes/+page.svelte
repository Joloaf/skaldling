<script lang="ts">
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroSummaryDto } from '$lib/api/heroes';
	import { spritesApi, type SpriteDto } from '$lib/api/sprites';
	import type { ProblemDetails } from '$lib/api/client';
	import { currentHero } from '$lib/stores/currentHero.svelte';

	type PageState =
		| { status: 'loading' }
		| { status: 'loaded'; heroes: HeroSummaryDto[]; catalog: SpriteDto[] }
		| { status: 'error'; problem: ProblemDetails };

	let pageState = $state<PageState>({ status: 'loading' });

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		pageState = { status: 'loading' };
		const [heroesResult, spritesResult] = await Promise.all([heroesApi.list(), spritesApi.list()]);
		if (!heroesResult.ok) {
			pageState = { status: 'error', problem: heroesResult.problem };
			return;
		}
		if (!spritesResult.ok) {
			pageState = { status: 'error', problem: spritesResult.problem };
			return;
		}
		pageState = {
			status: 'loaded',
			heroes: heroesResult.data.heroes,
			catalog: spritesResult.data.sprites
		};
	}

	function selectedInLayerOrder(avatarSpriteIds: string[], catalog: SpriteDto[]) {
		return avatarSpriteIds
			.map((id) => catalog.find((s) => s.id === id))
			.filter((s): s is SpriteDto => s !== undefined)
			.sort((a, b) => a.layer - b.layer);
	}
</script>

<h1>Your heroes</h1>

{#if pageState.status === 'loading'}
	<p>Loading…</p>
{:else if pageState.status === 'error'}
	<p style="color: crimson">Failed to load: {pageState.problem.title ?? 'unknown error'}</p>
{:else if pageState.status === 'loaded'}
	<p class="subtitle">
		{pageState.heroes.length === 0
			? 'No heroes yet — create your first one.'
			: 'Pick a hero to make them active, or create a new one.'}
	</p>

	<ul class="heroes-grid">
		{#each pageState.heroes as hero (hero.id)}
			<li>
				<a
					class="hero-card"
					class:active={hero.id === currentHero.id}
					href={resolve('/')}
					onclick={() => currentHero.set(hero.id)}
				>
					{#if hero.id === currentHero.id}
						<span class="badge">Active</span>
					{/if}
					<div class="avatar">
						{#each selectedInLayerOrder(hero.avatarSpriteIds, pageState.catalog) as sprite (sprite.id)}
							<img src={sprite.assetPath} alt={sprite.description} />
						{/each}
					</div>
					<p class="name">{hero.name}</p>
					<p class="meta">Reading age {hero.readingAge} · {hero.achievementPoints} pts</p>
				</a>
			</li>
		{/each}

		<li>
			<a class="hero-card create-card" href={resolve('/heroes/new')}>
				<span class="plus" aria-hidden="true">＋</span>
				<span class="name">New hero</span>
			</a>
		</li>
	</ul>
{/if}

<style>
    .subtitle {
        color: var(--text-muted);
        margin: 0.25rem 0 1.5rem;
    }

    .heroes-grid {
        list-style: none;
        padding: 0;
        margin: 0;
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
        gap: 1rem;
    }

    .hero-card {
        position: relative;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.3rem;
        min-height: 200px;
        padding: 1rem;
        border: 1px solid var(--border);
        border-radius: var(--radius, 12px);
        background: var(--surface);
        color: var(--text);
        text-decoration: none;
        transition: box-shadow 0.2s ease, border-color 0.2s ease;
    }
    .hero-card:hover,
    .hero-card:focus-visible {
        border-color: var(--accent);
        box-shadow:
                0 0 0 1px var(--accent),
                0 6px 24px color-mix(in srgb, var(--accent) 35%, transparent);
        outline: none;
    }
    .hero-card.active {
        border-color: var(--accent);
        box-shadow:
                0 0 0 2px var(--accent),
                0 8px 24px color-mix(in srgb, var(--accent) 35%, transparent);
    }

    .avatar {
        position: relative;
        width: 120px;
        height: 120px;
        background: #efe6d3;
        border-radius: 12px;
        overflow: hidden;
        box-shadow: inset 0 2px 7px rgba(0, 0, 0, 0.4);
    }
    .avatar img {
        position: absolute;
        inset: 0;
        width: 100%;
        height: 100%;
    }

    .name {
        margin: 0.3rem 0 0;
        font-weight: 700;
    }
    .meta {
        margin: 0;
        font-size: 0.85rem;
        color: var(--text-muted);
    }

    .badge {
        position: absolute;
        top: -0.6rem;
        right: 0.8rem;
        font-size: 0.7rem;
        font-weight: 700;
        letter-spacing: 0.05em;
        text-transform: uppercase;
        padding: 0.2rem 0.6rem;
        border-radius: 999px;
        background: var(--accent);
        color: var(--on-accent);
        box-shadow: 0 2px 7px rgba(0, 0, 0, 0.35);
    }

    .create-card {
        justify-content: center;
        border-style: dashed;
        color: var(--text-muted);
    }
    .create-card .plus {
        font-size: 2.2rem;
        line-height: 1;
    }
    .create-card:hover {
        color: var(--accent);
    }
</style>
