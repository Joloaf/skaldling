<script lang="ts">
	import { resolve } from '$app/paths';
	import { currentHero } from '$lib/stores/currentHero.svelte';
	import { heroesApi, type HeroDto } from '$lib/api/heroes';
	import { adventuresApi, type AdventureStatus } from '$lib/api/adventures';
	import type { ProblemDetails } from '$lib/api/client';
	import MenuButton from '$lib/components/menu/MenuButton.svelte';

	type MenuState =
		| { status: 'none' }
		| { status: 'loading' }
		| { status: 'loaded'; hero: HeroDto; adventureStatus: AdventureStatus | null }
		| { status: 'error'; problem: ProblemDetails };

	let menuState = $state<MenuState>(currentHero.id ? { status: 'loading' } : { status: 'none' });

	$effect(() => {
		const id = currentHero.id;
		if (!id) {
			menuState = { status: 'none' };
			return;
		}
		loadMenu(id);
	});

	async function loadMenu(id: string) {
		menuState = { status: 'loading' };

		const heroResult = await heroesApi.get(id);
		if (!heroResult.ok) {
			if (heroResult.problem.status === 404) {
				currentHero.clear();
				menuState = { status: 'none' };
			} else {
				menuState = { status: 'error', problem: heroResult.problem };
			}
			return;
		}
		const hero = heroResult.data;

		let adventureStatus: AdventureStatus | null = null;
		if (hero.activeAdventureId) {
			const adventureResult = await adventuresApi.get(hero.activeAdventureId);
			if (adventureResult.ok) {
				adventureStatus = adventureResult.data.status;
			}
		}
		menuState = { status: 'loaded', hero, adventureStatus };
	}

	const hero = $derived(menuState.status === 'loaded' ? menuState.hero : null);
	const canPlay = $derived(
		menuState.status === 'loaded' &&
		(menuState.adventureStatus === 'Ready' || menuState.adventureStatus === 'Active')
	);

	const playSubtitle = $derived(
		canPlay
			? 'Continue your journey'
			: hero?.activeAdventureId
				? 'Story not ready yet'
				: 'No adventure in progress yet'
	);

	type MenuItem = {
		key: string;
		title: string;
		subtitle: string;
		icon: string;
		href: string;
		disabled: boolean;
	};

	const items: MenuItem[] = $derived([
		{
			key: 'hero',
			title: hero ? hero.name : menuState.status === 'loading' ? 'Loading…' : 'No hero selected',
			subtitle: 'View / Edit hero',
			icon: 'hero',
			href: hero ? resolve('/heroes/[id]/edit', { id: hero.id }) : '',
			disabled: !hero
		},
		{
			key: 'heroes',
			title: 'Heroes',
			subtitle: 'Select / Create new hero',
			icon: 'heroes',
			href: resolve('/heroes'),
			disabled: false
		},
		{
			key: 'create',
			title: 'Create Adventure',
			subtitle: 'Write the script for a new adventure',
			icon: 'create',
			href: hero ? resolve('/heroes/[id]/new-adventure', { id: hero.id }) : '',
			disabled: !hero
		},
		{
			key: 'play',
			title: 'Play Adventure',
			subtitle: playSubtitle,
			icon: 'play',
			href: hero && canPlay ? resolve('/heroes/[id]/play', { id: hero.id }) : '',
			disabled: !canPlay
		},
		{
			key: 'library',
			title: 'Adventure Library',
			subtitle: 'Your completed quests',
			icon: 'library',
			href: hero ? resolve('/heroes/[id]/library', { id: hero.id }) : '',
			disabled: !hero
		}
	]);
</script>

<section class="menu">
	<header class="head">
		<h1>Skaldling</h1>
		<p class="tagline">Transform your tasks into epic adventures</p>
	</header>

	{#if menuState.status === 'error'}
		<p class="notice">
			Couldn't reach the server to load your hero. Pick one under <strong>Heroes</strong>, or try again.
		</p>
	{/if}

	<nav class="cards">
		{#each items as item (item.key)}
			<MenuButton
				title={item.title}
				subtitle={item.subtitle}
				icon={item.icon}
				href={item.href}
				disabled={item.disabled}
			/>
		{/each}
	</nav>
</section>

<style>
    .menu { max-width: 560px; margin: 0 auto; }
    .head { text-align: center; margin: 1rem 0 2rem; }
    .head h1 {
        margin: 0;
        font-family: var(--font-display);
        font-size: 6rem;
        font-weight: 400;
        letter-spacing: 0.01em;
    }
    .tagline { margin: 0.25rem 0 0; color: var(--text-muted, #6b5d4a); }
    .notice { text-align: center; color: var(--accent-warm, #b5482f); }
    .cards { display: flex; flex-direction: column; gap: 0.75rem; }
</style>
