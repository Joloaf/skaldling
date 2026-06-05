<script lang="ts">
	import { tick, untrack } from 'svelte';
	import { buildChunks, buildGroups, buildPages } from './bookChunks';
	import BookPage from './BookPage.svelte';
	import type { PlayDayDto } from '$lib/api/adventures';

	type Props = {
		days: PlayDayDto[];
		currentNodeId: string | null;
		navigateToNodeId?: string | null;
		onTaskToggle: (taskId: string, isCompleted: boolean) => void;
	};

	const { days, currentNodeId, navigateToNodeId = null, onTaskToggle }: Props = $props();

	const chunks = $derived(buildChunks(days, currentNodeId));
	const groups = $derived(buildGroups(chunks));

	let pageAssignments = $state<Map<string, number>>(new Map());
	let pageElements: HTMLElement[] = $state([]);
	let currentSpread = $state(0);
	let paginationRunId = 0;

	$effect(() => {
		void chunks.length;
		const myRunId = ++paginationRunId;
		runPagination(myRunId);
	});

	async function runPagination(runId: number) {
		const groupsSnapshot = untrack(() => groups);
		const chunksSnapshot = untrack(() => chunks);
		const newAssignments = new Map(untrack(() => pageAssignments));

		const allChunkIds = new Set(chunksSnapshot.map((c) => c.id));
		for (const id of Array.from(newAssignments.keys())) {
			if (!allChunkIds.has(id)) newAssignments.delete(id);
		}

		let currentWritePage =
			newAssignments.size === 0 ? 0 : Math.max(...Array.from(newAssignments.values()));

		for (const group of groupsSnapshot) {
			if (runId !== paginationRunId) return;

			const firstChunkId = group.chunks[0].id;
			if (newAssignments.has(firstChunkId)) {
				const p = newAssignments.get(firstChunkId)!;
				if (p > currentWritePage) currentWritePage = p;
				continue;
			}

			const currentPageHasContent = Array.from(newAssignments.values()).includes(currentWritePage);
			if (group.isChapterHeading && currentPageHasContent) {
				currentWritePage++;
			}

			for (const chunk of group.chunks) {
				newAssignments.set(chunk.id, currentWritePage);
			}
			pageAssignments = new Map(newAssignments);
			await tick();
			if (runId !== paginationRunId) return;

			const pageEl = pageElements[currentWritePage];
			if (pageEl && pageEl.scrollHeight > pageEl.clientHeight) {
				for (const chunk of group.chunks) {
					newAssignments.delete(chunk.id);
				}
				currentWritePage++;
				for (const chunk of group.chunks) {
					newAssignments.set(chunk.id, currentWritePage);
				}
				pageAssignments = new Map(newAssignments);
				await tick();
				if (runId !== paginationRunId) return;
			}
		}

		const targetSpread = Math.floor(currentWritePage / 2);
		untrack(() => {
			if (currentSpread !== targetSpread) currentSpread = targetSpread;
		});
	}

	const totalPages = $derived(
		pageAssignments.size === 0 ? 0 : Math.max(...Array.from(pageAssignments.values())) + 1
	);
	const totalSpreads = $derived(Math.max(1, Math.ceil(totalPages / 2)));
	const pagesContent = $derived(buildPages(chunks, pageAssignments, totalPages));

	$effect(() => {
		if (!navigateToNodeId) return;
		const narrId = `narr-${navigateToNodeId}`;
		const taskId = `task-${navigateToNodeId}`;
		let targetPage = pageAssignments.get(narrId);
		if (targetPage === undefined) targetPage = pageAssignments.get(taskId);
		if (targetPage === undefined) return;
		untrack(() => {
			currentSpread = Math.floor(targetPage! / 2);
		});
	});

	function prevSpread() {
		if (currentSpread > 0) currentSpread--;
	}
	function nextSpread() {
		if (currentSpread < totalSpreads - 1) currentSpread++;
	}
</script>

<div class="book-wrapper">
	<div class="book">
		{#if chunks.length === 0}
			<p class="empty-state">Your story will be written as you walk the path.</p>
		{:else}
			{#each pagesContent as pageContent, pageIdx}
				<div
					class="page"
					class:left={pageIdx === currentSpread * 2}
					class:right={pageIdx === currentSpread * 2 + 1}
					bind:this={pageElements[pageIdx]}>
					<BookPage chunks={pageContent} {onTaskToggle} />
				</div>
			{/each}
		{/if}
	</div>

	{#if totalPages > 2}
		<div class="pagination">
			<button class="page-btn" onclick={prevSpread} disabled={currentSpread === 0}>← Previous</button>
			<span class="page-indicator">
				Pages {currentSpread * 2 + 1}–{Math.min(currentSpread * 2 + 2, totalPages)} of {totalPages}
			</span>
			<button class="page-btn" onclick={nextSpread} disabled={currentSpread >= totalSpreads - 1}>Next →</button>
		</div>
	{/if}
</div>

<style>
    .book-wrapper {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        width: 800px;
        max-width: 100%;
    }

    .book {
        position: relative;
        width: 800px;
        height: 800px;
        max-width: 100%;
        background: #f5e8c4;
        border: 1px solid #b39752;
        border-radius: 6px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
        font-family: Georgia, serif;
        color: #3a2f1a;
        line-height: 1.7;
        overflow: hidden;
    }

    .book::before {
        content: '';
        position: absolute;
        left: 50%;
        top: 1.5rem;
        bottom: 1.5rem;
        width: 1px;
        background: #b39752;
        z-index: 1;
        pointer-events: none;
    }

    .page {
        position: absolute;
        width: 400px;
        height: 800px;
        padding: 2.5rem 2.5rem;
        box-sizing: border-box;
        top: 0;
        left: -99999px;
        overflow: hidden;
        visibility: hidden;
    }
    .page.left {
        left: 0;
        visibility: visible;
    }
    .page.right {
        left: 400px;
        visibility: visible;
    }

    .empty-state {
        font-style: italic;
        color: #888;
        text-align: center;
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        margin: 0;
        padding: 0 2rem;
    }

    .pagination {
        display: flex;
        justify-content: space-between;
        align-items: center;
        width: 100%;
    }
    .page-btn {
        padding: 0.4rem 0.9rem;
        background: #f5e8c4;
        border: 1px solid #b39752;
        border-radius: 4px;
        cursor: pointer;
        font-family: Georgia, serif;
        color: #3a2f1a;
        font-size: 0.9rem;
    }
    .page-btn:hover:not(:disabled) {
        background: #ede0a8;
    }
    .page-btn:disabled {
        opacity: 0.4;
        cursor: not-allowed;
    }
    .page-indicator {
        font-family: Georgia, serif;
        color: #5b3a29;
        font-size: 0.85rem;
        font-variant: small-caps;
    }

    @media (max-width: 1024px) {
        .book-wrapper {
            width: 100%;
        }
        .book {
            width: 100%;
        }
        .page {
            width: 50%;
        }
        .page.right {
            left: 50%;
        }
    }
</style>