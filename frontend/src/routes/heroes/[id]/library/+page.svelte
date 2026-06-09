<script lang="ts">
	import { page } from '$app/state';
	import { heroesApi } from '$lib/api/heroes';
	import { adventuresApi } from '$lib/api/adventures';
	import type { ProblemDetails } from '$lib/api/client';

	type CompletedAdventure = {
		id: string;
		title: string;
		completedAt: string;
		themeName?: string;
		tone?: string;
		finaleReward?: string | null;
		dayCount?: number;
		taskCount?: number;
	};

	type LoadState =
		| { status: 'loading' }
		| { status: 'loaded'; heroName: string; adventures: CompletedAdventure[] }
		| { status: 'error'; problem: ProblemDetails };

	let loadState = $state<LoadState>({ status: 'loading' });

	const heroId = page.params.id!;

	$effect(() => {
		loadLibrary();
	});

	async function loadLibrary() {
		loadState = { status: 'loading' };
		const heroResult = await heroesApi.get(heroId);
		if (!heroResult.ok) {
			loadState = { status: 'error', problem: heroResult.problem };
			return;
		}
		const hero = heroResult.data;

		const adventures = await Promise.all(
			hero.pastAdventures.map(async (past): Promise<CompletedAdventure> => {
				const result = await adventuresApi.get(past.id);
				if (!result.ok) {
					return { id: past.id, title: past.title, completedAt: past.completedAt };
				}
				const adv = result.data;
				return {
					id: past.id,
					title: past.title,
					completedAt: past.completedAt,
					themeName: adv.themeName,
					tone: adv.tone,
					finaleReward: adv.finaleReward,
					dayCount: adv.days.length,
					taskCount: adv.days.reduce((sum, d) => sum + d.nodes.length, 0)
				};
			})
		);

		loadState = { status: 'loaded', heroName: hero.name, adventures };
	}

	function formatDate(iso: string): string {
		return new Date(iso).toLocaleDateString();
	}
</script>

<svelte:head>
	<title>Adventure Library - Skaldling</title>
</svelte:head>

<h1>Adventure Library</h1>

{#if loadState.status === 'loading'}
	<p>Loading…</p>
{:else if loadState.status === 'error'}
	<p style="color: crimson">Failed to load: {loadState.problem.title ?? 'unknown error'}</p>
{:else if loadState.status === 'loaded'}
	{#if loadState.adventures.length === 0}
		<p class="empty">
			{loadState.heroName} hasn't completed any adventures yet. Finish one and it'll be kept here.
		</p>
	{:else}
		<p class="subtitle">{loadState.heroName}'s completed quests</p>
		<ul class="library">
			{#each loadState.adventures as adv (adv.id)}
				<li class="adv-card">
					<h2 class="adv-title">{adv.title}</h2>
					{#if adv.themeName || adv.tone}
						<div class="tags">
							{#if adv.themeName}<span class="tag">{adv.themeName}</span>{/if}
							{#if adv.tone}<span class="tag">{adv.tone}</span>{/if}
						</div>
					{/if}
					{#if adv.taskCount != null && adv.dayCount != null}
						<p class="adv-meta">{adv.taskCount} tasks across {adv.dayCount} days</p>
					{/if}
					{#if adv.finaleReward}
						<p class="adv-reward">Reward: <strong>{adv.finaleReward}</strong></p>
					{/if}
					<p class="adv-completed">Completed {formatDate(adv.completedAt)}</p>
				</li>
			{/each}
		</ul>
	{/if}
{/if}

<style>
    h1 {
        margin-top: 0;
    }
    .subtitle {
        color: var(--text-muted);
        margin: 0.25rem 0 1.5rem;
    }
    .empty {
        color: var(--text-muted);
    }

    .library {
        list-style: none;
        padding: 0;
        margin: 0;
        display: flex;
        flex-direction: column;
        gap: 1rem;
    }
    .adv-card {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: var(--radius, 12px);
        padding: 1.25rem 1.5rem;
    }
    .adv-title {
        margin: 0;
        font-size: 1.3rem;
    }
    .tags {
        display: flex;
        gap: 0.5rem;
        flex-wrap: wrap;
        margin: 0.6rem 0;
    }
    .tag {
        font-size: 0.78rem;
        font-weight: 600;
        padding: 0.2rem 0.65rem;
        border-radius: 999px;
        background: var(--accent-tint, rgba(70, 170, 176, 0.18));
        color: var(--accent);
    }
    .adv-meta {
        margin: 0.25rem 0;
        color: var(--text-muted);
        font-size: 0.9rem;
    }
    .adv-reward {
        margin: 0.25rem 0;
        color: var(--gold);
        font-size: 0.95rem;
    }
    .adv-reward strong {
        color: var(--text);
    }
    .adv-completed {
        margin: 0.6rem 0 0;
        color: var(--text-muted);
        font-size: 0.85rem;
    }
</style>
