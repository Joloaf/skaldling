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
	import type { FlatNode, ProgressState } from '$lib/components/play/types';
	import PlayMap from '$lib/components/play/PlayMap.svelte';
	import CompletionCelebration from '$lib/components/play/CompletionCelebration.svelte';
	import StoryBook from '$lib/components/play/StoryBook.svelte';

	type LoadState =
		| { status: 'loading' }
		| { status: 'noActiveAdventure'; hero: HeroDto }
		| { status: 'loaded'; hero: HeroDto; adventure: GetAdventureResponse }
		| { status: 'error'; problem: ProblemDetails };

	const heroId = page.params.id!;

	let loadState = $state<LoadState>({ status: 'loading' });
	let lastEarnedScore = $state<number | null>(null);
	let navigateToNodeId = $state<string | null>(null);

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

	const flatNodes: FlatNode[] = $derived.by(() => {
		if (loadState.status !== 'loaded') return [];
		const list: FlatNode[] = [];
		let flatIndex = 0;
		let foundCurrent = false;

		for (const day of loadState.adventure.days) {
			day.nodes.forEach((node, nodeIndexInDay) => {
				let state: ProgressState;
				if (node.adventureTask.isCompleted) {
					state = 'completed';
				} else if (!foundCurrent) {
					state = 'current';
					foundCurrent = true;
				} else {
					state = 'locked';
				}
				list.push({
					node,
					dayNumber: day.dayNumber,
					nodeIndexInDay,
					flatIndex: flatIndex++,
					state,
					isFirstOfDay: nodeIndexInDay === 0
				});
			});
		}
		return list;
	});

	const currentNode: FlatNode | null = $derived(
		flatNodes.find((fn) => fn.state === 'current') ?? null
	);

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
		flatNodes.length > 0 && flatNodes.every((fn) => fn.state === 'completed')
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

	function handleMapNodeClick(nodeId: string) {
		navigateToNodeId = null;
		setTimeout(() => (navigateToNodeId = nodeId), 0);
	}
</script>

<svelte:head>
	<title>{loadState.status === 'loaded' ? loadState.adventure.title : 'Play'} - Skaldling</title>
</svelte:head>

{#if loadState.status === 'loading'}
	<p>Loading…</p>
{:else if loadState.status === 'error'}
	<p style="color: crimson">Failed to load: {loadState.problem.title ?? 'unknown error'}</p>
{:else if loadState.status === 'noActiveAdventure'}
	<h1>{loadState.hero.name}</h1>
	<p>No adventure in flight.</p>
	<p><a href={resolve('/heroes/[id]/new-adventure', { id: heroId })}>Start a new adventure</a></p>
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
		<p class="view-switch"><a href={resolve('/heroes/[id]/play', { id: heroId })}>Switch to list view</a></p>
	</header>

	{#if adventure.status === 'Ready'}
		<section class="ready-card">
			<h2>The path awaits</h2>
			<p class="ready-intro">{adventure.days[0]?.narrativeIntro ?? '(no intro)'}</p>
			<button class="start-btn" onclick={handleStart}>Begin the journey</button>
		</section>
	{:else}
		<div class="play-layout">
			<div class="map-section">
				<PlayMap {flatNodes} onNodeClick={handleMapNodeClick} />

				{#if adventure.status === 'Active' && allTasksComplete}
					<section class="finale-cta">
						<h2>The journey ends</h2>
						{#if adventure.finaleReward}
							<p>Finale reward: <strong>{adventure.finaleReward}</strong></p>
						{/if}
						<button class="finale-btn" onclick={handleConfirmFinale}>Confirm reward delivered</button>
					</section>
				{:else if adventure.status === 'Completed'}
					<CompletionCelebration
						heroName={hero.name}
						earnedScore={lastEarnedScore}
						lifetimePoints={hero.achievementPoints}
						{heroId} />
				{/if}
			</div>

			<StoryBook
				days={adventure.days}
				currentNodeId={currentNode?.node.id ?? null}
				{navigateToNodeId}
				onTaskToggle={handleTaskToggle} />
		</div>
	{/if}
{/if}

<style>
    .play-header {
        margin-bottom: 1rem;
        padding-bottom: 0.75rem;
        border-bottom: 1px solid #ddd;
        max-width: 1400px;
        margin-left: auto;
        margin-right: auto;
    }
    .play-header h1 {
        margin: 0;
    }
    .play-meta {
        margin: 0.5rem 0 0;
        color: #666;
    }
    .view-switch {
        margin: 0.5rem 0 0;
        font-size: 0.85rem;
    }
    .view-switch a {
        color: #888;
        text-decoration: none;
    }
    .view-switch a:hover {
        text-decoration: underline;
    }

    .ready-card {
        max-width: 600px;
        margin: 0 auto;
        padding: 1.5rem;
        border: 1px solid #ddd;
        border-radius: 6px;
        background: #fafafa;
        text-align: center;
    }
    .ready-card h2 {
        margin-top: 0;
    }
    .ready-intro {
        font-style: italic;
        color: #555;
    }
    .start-btn {
        margin-top: 1rem;
        padding: 0.6rem 1.4rem;
        background: forestgreen;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 1rem;
    }

    .play-layout {
        display: grid;
        grid-template-columns: minmax(320px, 500px) 1fr;
        gap: 1.5rem;
        align-items: start;
        max-width: 1400px;
        margin: 0 auto;
    }

    .map-section {
        position: sticky;
        top: 1rem;
    }

    .finale-cta {
        margin-top: 1.5rem;
        padding: 1.5rem;
        background: linear-gradient(to bottom, #fffcf0, #fff8e0);
        border: 2px solid goldenrod;
        border-radius: 8px;
        text-align: center;
    }
    .finale-cta h2 {
        margin-top: 0;
        color: #8b6f00;
    }
    .finale-btn {
        margin-top: 1rem;
        padding: 0.75rem 1.5rem;
        background: goldenrod;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 1.1rem;
    }

    @media (max-width: 1024px) {
        .play-layout {
            grid-template-columns: 1fr;
        }
        .map-section {
            position: static;
        }
    }
</style>