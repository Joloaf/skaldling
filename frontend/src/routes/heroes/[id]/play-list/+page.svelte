<script lang="ts">
	import { page } from '$app/state';
	import { resolve } from '$app/paths';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import {
		adventuresApi,
		type GetAdventureResponse,
		type PlayAdventureTaskDto
	} from '$lib/api/adventures';
	import type { ProblemDetails } from '$lib/api/client';
	import DayCard from '$lib/components/play/DayCard.svelte';
	import CompletionCelebration from '$lib/components/play/CompletionCelebration.svelte';
	import type { ProgressState } from '$lib/components/play/types';
	import StoryLoader from '$lib/components/StoryLoader.svelte';
	import Icon from '$lib/components/Icon.svelte';

	type LoadState =
		| { status: 'loading' }
		| { status: 'noActiveAdventure'; hero: HeroDto }
		| { status: 'loaded'; hero: HeroDto; adventure: GetAdventureResponse }
		| { status: 'error'; problem: ProblemDetails };

	const heroId = page.params.id!;

	let loadState = $state<LoadState>({ status: 'loading' });
	let lastEarnedScore = $state<number | null>(null);
	let generating = $state(false);

	$effect(() => {
		loadEverything();
	});

	async function loadEverything() {
		loadState = { status: 'loading' };
		const heroResult = await heroesApi.get(heroId);
		if (!heroResult.ok) {
			loadState = { status: 'error', problem: heroResult.problem };
			return;
		}
		const hero = heroResult.data;
		if (!hero.activeAdventureId) {
			loadState = { status: 'noActiveAdventure', hero };
			return;
		}
		const adventureResult = await adventuresApi.get(hero.activeAdventureId);
		if (!adventureResult.ok) {
			loadState = { status: 'error', problem: adventureResult.problem };
			return;
		}
		loadState = { status: 'loaded', hero, adventure: adventureResult.data };
	}

	const dayStates: ProgressState[] = $derived.by(() => {
		if (loadState.status !== 'loaded') return [];
		const days = loadState.adventure.days;
		return days.map((day, idx): ProgressState => {
			const allTasksComplete = day.nodes.every((n) => n.adventureTask.isCompleted);
			if (allTasksComplete) return 'completed';
			const earlierAllComplete = days
				.slice(0, idx)
				.every((d) => d.nodes.every((n) => n.adventureTask.isCompleted));
			return earlierAllComplete ? 'current' : 'locked';
		});
	});

	const adventureScore: number = $derived(
		loadState.status === 'loaded'
			? loadState.adventure.days
				.flatMap((d) => d.nodes)
				.filter((n) => n.adventureTask.isCompleted)
				.reduce((sum, n) => sum + n.adventureTask.pointValue, 0)
			: 0
	);

	const maxAdventureScore: number = $derived(
		loadState.status === 'loaded'
			? loadState.adventure.days
				.flatMap((d) => d.nodes)
				.reduce((sum, n) => sum + n.adventureTask.pointValue, 0)
			: 0
	);

	const allTasksComplete: boolean = $derived(
		loadState.status === 'loaded'
			? loadState.adventure.days
				.flatMap((d) => d.nodes)
				.every((n) => n.adventureTask.isCompleted)
			: false
	);

	function findTask(
		adventure: GetAdventureResponse,
		taskId: string
	): PlayAdventureTaskDto | undefined {
		for (const day of adventure.days) {
			for (const node of day.nodes) {
				if (node.adventureTask.id === taskId) return node.adventureTask;
			}
		}
		return undefined;
	}

	async function handleStart() {
		if (loadState.status !== 'loaded') return;
		const result = await adventuresApi.start(loadState.adventure.id);
		if (result.ok) {
			loadState.adventure.status = result.data.status;
			window.scrollTo({ top: 0, behavior: 'smooth' });
		}
	}

	async function handleTaskToggle(taskId: string, isCompleted: boolean) {
		if (loadState.status !== 'loaded') return;
		const result = await adventuresApi.updateTaskCompletion(
			loadState.adventure.id,
			taskId,
			{ isCompleted }
		);
		if (result.ok) {
			const task = findTask(loadState.adventure, taskId);
			if (task) task.isCompleted = result.data.isCompleted;
		}
	}

	async function handleConfirmFinale() {
		if (loadState.status !== 'loaded') return;
		const result = await adventuresApi.confirmFinale(loadState.adventure.id);
		if (result.ok) {
			loadState.adventure.status = result.data.status;
			loadState.hero.achievementPoints = result.data.heroAchievementPoints;
			lastEarnedScore = result.data.earnedScore;
			window.scrollTo({ top: 0, behavior: 'smooth' });
		}
	}

	async function handleGenerate() {
		if (loadState.status !== 'loaded') return;
		generating = true;
		const result = await adventuresApi.generate(loadState.adventure.id);
		if (result.ok) {
			const refreshed = await adventuresApi.get(loadState.adventure.id);
			if (refreshed.ok && loadState.status === 'loaded') {
				loadState.adventure = refreshed.data;
			}
			window.scrollTo({ top: 0, behavior: 'smooth' });
		}
		generating = false;
	}
</script>

<svelte:head>
	<title>{loadState.status === 'loaded' ? loadState.adventure.title : 'Play'} - Skaldling</title>
</svelte:head>

{#if loadState.status === 'loading'}
	<p>Loading…</p>
{:else if loadState.status === 'error'}
	<p style="color: var(--accent-warm)">Failed to load: {loadState.problem.title ?? 'unknown error'}</p>
{:else if loadState.status === 'noActiveAdventure'}
	<h1>{loadState.hero.name}</h1>
	<p>No adventure in flight.</p>
	<p class="text-link"><a href={resolve('/heroes/[id]/new-adventure', { id: heroId })}>Start a new adventure</a></p>
{:else if loadState.status === 'loaded'}
	{@const adventure = loadState.adventure}
	{@const hero = loadState.hero}

	<header class="play-header">
		<h1>{adventure.title}</h1>
		<p class="play-meta">
			{hero.name} · {hero.achievementPoints} lifetime achievement points
			{#if adventure.status === 'Active' || adventure.status === 'Completed'}
				<span style="margin-left: 1rem;">Adventure score: <strong>{adventureScore} / {maxAdventureScore}</strong></span>
			{/if}
		</p>
		<p class="view-switch"><a href={resolve('/heroes/[id]/play', { id: heroId })}>Switch to map view</a></p>
	</header>

	{#if adventure.status === 'Draft' || adventure.status === 'Generating'}
		{#if generating || adventure.status === 'Generating'}
			<StoryLoader />
		{:else}
			<section class="not-ready">
				<h2>This adventure hasn't been written yet</h2>
				<p>Bring {hero.name}'s story to life to start playing.</p>
				<button class="start-btn" onclick={handleGenerate}>Write the story</button>
			</section>
		{/if}
	{:else if adventure.status === 'Ready'}
		<section class="ready-card">
			<h2 style="margin-top: 0;">The path awaits</h2>
			<p style="font-style: italic; color: var(--text-muted);">{adventure.days[0]?.narrativeIntro ?? '(no intro)'}</p>
			<button class="start-btn" onclick={handleStart}>Begin the journey</button>
		</section>
	{:else if adventure.status === 'Active'}
		{#each adventure.days as day, idx (day.id)}
			<DayCard
				{day}
				state={dayStates[idx]}
				onTaskToggle={handleTaskToggle} />
		{/each}

		{#if allTasksComplete}
			<section class="finale-cta">
				<span class="finale-trophy"><Icon name="trophy" size={40} /></span>
				<h2>The journey ends</h2>
				{#if adventure.finaleReward}
					<p class="finale-reward">Finale reward: <strong>{adventure.finaleReward}</strong></p>
				{/if}
				<button class="finale-btn" onclick={handleConfirmFinale}>Confirm reward delivered</button>
			</section>
		{/if}
	{:else if adventure.status === 'Completed'}
		<CompletionCelebration
			heroName={hero.name}
			earnedScore={lastEarnedScore}
			lifetimePoints={hero.achievementPoints}
			{heroId} />

		{#each adventure.days as day, idx (day.id)}
			<DayCard
				{day}
				state={dayStates[idx]}
				readOnly={true}
				onTaskToggle={handleTaskToggle} />
		{/each}
	{/if}
{/if}

<style>
    .text-link a {
				color: var(--accent); text-decoration: none;
		}
    .text-link a:hover {
				text-decoration: underline;
		}

    .play-header {
        margin-bottom: 1.5rem;
        padding-bottom: 1rem;
        border-bottom: 1px solid var(--border);
    }
    .play-header h1 {
        margin: 0;
    }
    .play-meta {
        margin: 0.5rem 0 0;
        color: var(--text-muted);
    }

    .view-switch {
        margin: 0.5rem 0 0;
        font-size: 0.85rem;
    }
    .view-switch a {
        color: var(--accent);
        text-decoration: none;
    }
    .view-switch a:hover {
        text-decoration: underline;
    }

    .ready-card {
        padding: 1rem;
        border: 1px solid var(--border);
        border-radius: var(--radius, 12px);
        background: var(--surface);
    }
    .start-btn {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        margin-top: 1rem;
        padding: 0.5rem 1rem;
        border: 1px solid var(--border);
        border-radius: 10px;
        background: var(--surface-alt);
        color: var(--text);
        font-weight: 700;
        font-size: 1rem;
        cursor: pointer;
        transition: border-color 0.15s, color 0.15s, box-shadow 0.15s, transform 0.05s;
    }
    .start-btn:hover, .start-btn:focus-visible {
        border-color: var(--accent);
        color: var(--accent);
        box-shadow: 0 0 0 1px var(--accent), 0 6px 24px color-mix(in srgb, var(--accent) 38%, transparent);
        outline: none;
    }
    .start-btn:active { transform: scale(0.98); }
    .start-btn:disabled { opacity: 0.5; cursor: default; }

    .finale-cta {
        margin-top: 1.5rem;
        padding: 1.75rem;
        border-radius: var(--radius, 12px);
        background: var(--surface);
        border: 1px solid color-mix(in srgb, var(--gold) 45%, var(--border));
        box-shadow:
                0 0 0 1px color-mix(in srgb, var(--gold) 30%, transparent),
                0 10px 34px color-mix(in srgb, var(--gold) 20%, transparent);
        text-align: center;
        color: var(--text);
    }
    .finale-trophy { display: inline-flex; color: var(--gold); }
    .finale-cta h2 { margin: 0.5rem 0 0.5rem; color: var(--gold); }
    .finale-reward { margin: 0; color: var(--text-muted); }
    .finale-reward strong { color: var(--gold); }

    .finale-btn {
        margin-top: 1.25rem;
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.7rem 1.6rem;
        border: 1px solid color-mix(in srgb, var(--gold) 55%, var(--border));
        border-radius: 10px;
        background: color-mix(in srgb, var(--gold) 12%, var(--surface));
        color: var(--gold);
        font-size: 1.05rem;
        font-weight: 700;
        cursor: pointer;
        transition: border-color 0.15s, box-shadow 0.15s, transform 0.08s;
    }
    .finale-btn:hover, .finale-btn:focus-visible {
        border-color: var(--gold);
        box-shadow: 0 0 0 1px var(--gold), 0 6px 22px color-mix(in srgb, var(--gold) 40%, transparent);
        outline: none;
    }
    .finale-btn:active { transform: scale(0.97); }

    .not-ready {
        max-width: 600px;
        margin: 0 auto;
        padding: 1.5rem;
        border: 1px solid var(--border);
        border-radius: var(--radius, 12px);
        background: var(--surface);
        text-align: center;
    }
    .not-ready h2 { margin-top: 0; }
</style>