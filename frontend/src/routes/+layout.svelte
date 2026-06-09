<script lang="ts">
	import '../app.css';
	import '@fontsource/medievalsharp';
	import { page } from '$app/state';
	import favicon from '$lib/assets/favicon.svg';
	import BackButton from '$lib/components/menu/BackButton.svelte';

	let { children } = $props();

	const showBackButton = $derived(page.url.pathname !== '/');
	const wide = $derived(page.route.id === '/heroes/[id]/play');

	const PANELED = [
		'/heroes/new',
		'/heroes/[id]/details',
		'/heroes/[id]/avatar-builder',
		'/heroes/[id]/edit',
		'/heroes/[id]/new-adventure'
	];
	const paneled = $derived(PANELED.includes(page.route.id ?? ''));
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<main class="app-main" class:wide>
	{#if showBackButton}
		<BackButton />
	{/if}

	{#if paneled}
		<div class="panel">
			{@render children()}
		</div>
	{:else}
		{@render children()}
	{/if}
</main>

<style>
    .app-main {
        width: 100%;
        max-width: 760px;
        margin: 0 auto;
        padding: 1.5rem 1.25rem 3rem;
        box-sizing: border-box;
    }
    .app-main.wide {
        max-width: 1400px;
    }

		.panel {
        background: var(--surface);
        border: 1px solid var(--border);
        border-radius: var(--radius, 12px);
        padding: 1.5rem 1.75rem;
		}
</style>
